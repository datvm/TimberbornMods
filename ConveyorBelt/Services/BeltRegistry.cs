namespace ConveyorBelt.Services;

[BindSingleton]
public class BeltRegistry(BeltLinks links, EventBus eventBus) : ILoadableSingleton
{
    readonly Dictionary<Vector3Int, BeltCarrier> carriers = [];
    readonly List<BeltCarrier> carrierList = [];
    readonly List<BeltMerger> mergers = [];
    readonly List<BeltSplitter> splitters = [];
    readonly List<BeltLift> lifts = [];
    readonly List<BeltInventory> inventories = [];
    readonly Dictionary<Vector3Int, int> portTargets = [];
    bool dirty = true;

    public BeltSimulation Simulation { get; } = new();

    public void Load() => eventBus.Register(this);

    public void Register(BeltCarrier carrier)
    {
        carriers[carrier.Coordinates] = carrier;
        carrierList.Add(carrier);
        Simulation.Add(carrier.Sim);
        Note(carrier.InPort.Target, 1);
        Note(carrier.OutPort.Target, 1);
        dirty = true;
    }

    public void Unregister(BeltCarrier carrier)
    {
        if (carriers.TryGetValue(carrier.Coordinates, out var current) && current == carrier)
        {
            carriers.Remove(carrier.Coordinates);
        }

        carrierList.Remove(carrier);
        Simulation.Remove(carrier.Sim);
        Note(carrier.InPort.Target, -1);
        Note(carrier.OutPort.Target, -1);
        dirty = true;
    }

    public void Register(BeltMerger merger)
    {
        if (!Track(mergers, merger))
        {
            return;
        }

        Simulation.Add(merger.Sim);
        Simulation.Add(merger.Sim.Belt);
    }

    public void Unregister(BeltMerger merger)
    {
        if (!Drop(mergers, merger))
        {
            return;
        }

        Simulation.Remove(merger.Sim);
        Simulation.Remove(merger.Sim.Belt);
    }

    public void Register(BeltSplitter splitter)
    {
        if (!Track(splitters, splitter))
        {
            return;
        }

        Simulation.Add(splitter.Sim);
        Simulation.Add(splitter.Sim.Belt);
    }

    public void Unregister(BeltSplitter splitter)
    {
        if (!Drop(splitters, splitter))
        {
            return;
        }

        Simulation.Remove(splitter.Sim);
        Simulation.Remove(splitter.Sim.Belt);
    }

    public void Register(BeltLift lift)
    {
        if (!Track(lifts, lift))
        {
            return;
        }

        Simulation.Add(lift.Sim);
        Simulation.Add(lift.Sim.Belt);
    }

    public void Unregister(BeltLift lift)
    {
        if (!Drop(lifts, lift))
        {
            return;
        }

        Simulation.Remove(lift.Sim);
        Simulation.Remove(lift.Sim.Belt);
    }

    public void Register(BeltInventory inventory)
    {
        if (!inventories.Contains(inventory))
        {
            inventories.Add(inventory);
        }
    }

    public void Unregister(BeltInventory inventory) => inventories.Remove(inventory);

    public void Invalidate() => dirty = true;

    public void Prepare()
    {
        if (dirty)
        {
            Wire();
        }

        for (var i = 0; i < carrierList.Count; i++)
        {
            var carrier = carrierList[i];
            carrier.Sim.Running = carrier.Running;
            carrier.Sim.ItemsPerHour = carrier.ItemsPerHour;
        }

        for (var i = 0; i < mergers.Count; i++)
        {
            mergers[i].Sim.Running = mergers[i].Running;
        }

        for (var i = 0; i < splitters.Count; i++)
        {
            splitters[i].Sim.Running = splitters[i].Running;
        }

        for (var i = 0; i < lifts.Count; i++)
        {
            lifts[i].Sim.Running = lifts[i].Running;
        }
    }

    public void PublishStuck()
    {
        for (var i = 0; i < inventories.Count; i++)
        {
            inventories[i].ApplyStuck();
        }
    }

    [OnEvent]
    public void OnEnteredFinishedState(EnteredFinishedStateEvent e) => Touch(e.BlockObject);

    [OnEvent]
    public void OnExitedFinishedState(ExitedFinishedStateEvent e) => Touch(e.BlockObject);

    [OnEvent]
    public void OnEntityDeleted(EntityDeletedEvent e)
    {
        if (e.Entity.GetComponentOrNull<BlockObject>() is { } block)
        {
            Touch(block);
        }
    }

    bool Track<T>(List<T> list, T item)
    {
        if (list.Contains(item))
        {
            return false;
        }

        list.Add(item);
        dirty = true;
        return true;
    }

    bool Drop<T>(List<T> list, T item)
    {
        if (!list.Remove(item))
        {
            return false;
        }

        dirty = true;
        return true;
    }

    void Wire()
    {
        dirty = false;
        for (var i = 0; i < mergers.Count; i++)
        {
            mergers[i].Sim.ClearInputs();
        }

        for (var i = 0; i < lifts.Count; i++)
        {
            lifts[i].Sim.ClearInputs();
        }

        foreach (var carrier in carrierList)
        {
            carrier.Sim.Output = links.Resolve(new(carrier.Coordinates, carrier.OutPort.Target), carrier.Sim);
            carrier.Sim.Input = links.Resolve(new(carrier.Coordinates, carrier.InPort.Target), null) as ISimSource;
        }

        for (var i = 0; i < mergers.Count; i++)
        {
            var merger = mergers[i];
            merger.Sim.Belt.Output = One(merger.Coordinates, merger.OutputCells, merger.Sim.Belt);
        }

        for (var i = 0; i < splitters.Count; i++)
        {
            var splitter = splitters[i];
            splitter.Sim.Attach(Many(splitter.Coordinates, splitter.OutputCells, splitter.Sim.Belt));
        }

        for (var i = 0; i < lifts.Count; i++)
        {
            var lift = lifts[i];
            if (lift.SendingOut)
            {
                lift.Sim.AttachOut(One(lift.Coordinates, lift.OutputCells, lift.Sim.Belt));
            }
            else
            {
                lift.Sim.AttachSplit(Many(lift.Coordinates, lift.OutputCells, lift.Sim.Belt));
            }
        }

        Simulation.Invalidate();
    }

    ISimLink One(Vector3Int from, Vector3Int[] cells, SimBelt lane)
    {
        if (cells.Length == 0)
        {
            return SimLinks.None;
        }

        return links.Resolve(new(from, cells[0]), lane) ?? SimLinks.None;
    }

    IReadOnlyList<ISimLink> Many(Vector3Int from, Vector3Int[] cells, SimBelt lane)
    {
        var found = new ISimLink[cells.Length];
        for (var i = 0; i < cells.Length; i++)
        {
            found[i] = links.Resolve(new(from, cells[i]), lane) ?? SimLinks.None;
        }

        return found;
    }

    void Touch(BlockObject block)
    {
        if (dirty || !block.Positioned)
        {
            return;
        }

        foreach (var cell in block.PositionedBlocks.GetOccupiedCoordinates())
        {
            if (!portTargets.ContainsKey(cell))
            {
                continue;
            }

            dirty = true;
            return;
        }
    }

    void Note(Vector3Int cell, int delta)
    {
        portTargets.TryGetValue(cell, out var count);
        count += delta;
        if (count <= 0)
        {
            portTargets.Remove(cell);
        }
        else
        {
            portTargets[cell] = count;
        }
    }
}
