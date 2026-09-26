namespace ConveyorBelt.Services;

[BindSingleton]
public class BeltRegistry(IBlockService blocks, EventBus eventBus) : ILoadableSingleton
{
    readonly Dictionary<Vector3Int, BeltCarrier> carriers = [];
    readonly Dictionary<BeltCarrier, int> indexOf = [];
    readonly List<BeltCarrier> carrierList = [];
    readonly List<BeltTeleporter> teleporters = [];
    readonly List<BeltCarrier> moveOrder = [];
    readonly List<int> indegree = [];
    readonly List<int> queue = [];
    readonly List<List<int>> upstreams = [];
    bool dirty = true;

    public IReadOnlyList<BeltCarrier> MoveOrder
    {
        get
        {
            Ensure();
            return moveOrder;
        }
    }

    public IReadOnlyList<BeltTeleporter> Teleporters => teleporters;

    public void Load() => eventBus.Register(this);

    public void Register(BeltCarrier carrier)
    {
        carriers[carrier.Coordinates] = carrier;
        carrierList.Add(carrier);
        dirty = true;
    }

    public void Unregister(BeltCarrier carrier)
    {
        if (carriers.TryGetValue(carrier.Coordinates, out var current) && current == carrier)
        {
            carriers.Remove(carrier.Coordinates);
        }

        carrierList.Remove(carrier);
        dirty = true;
    }

    public void Register(BeltTeleporter teleporter)
    {
        if (!teleporters.Contains(teleporter))
        {
            teleporters.Add(teleporter);
        }
    }

    public void Unregister(BeltTeleporter teleporter) => teleporters.Remove(teleporter);

    public BeltCarrier? GiverAt(Vector3Int cell, Vector3Int destination)
    {
        if (!carriers.TryGetValue(cell, out var carrier) || !carrier.Running || !carrier.OutTargets(destination))
        {
            return null;
        }

        return carrier;
    }

    public BeltCarrier? ReceiverAt(Vector3Int cell, Vector3Int source)
    {
        if (!carriers.TryGetValue(cell, out var carrier) || !carrier.InTargets(source))
        {
            return null;
        }

        return carrier;
    }

    [OnEvent]
    public void OnEnteredFinishedState(EnteredFinishedStateEvent _) => dirty = true;

    [OnEvent]
    public void OnExitedFinishedState(ExitedFinishedStateEvent _) => dirty = true;

    [OnEvent]
    public void OnEntityDeleted(EntityDeletedEvent _) => dirty = true;

    void Ensure()
    {
        if (!dirty)
        {
            return;
        }

        dirty = false;
        foreach (var carrier in carrierList)
        {
            carrier.OutputCarrier = null;
            carrier.OutputBuilding = null;
            carrier.OutputInventories = null;
            carrier.InputBuilding = null;
            carrier.InputInventories = null;
            LinkOutput(carrier);
            LinkInput(carrier);
        }

        BuildOrder();
    }

    void LinkOutput(BeltCarrier carrier)
    {
        var target = carrier.OutPort.Target;
        if (carriers.TryGetValue(target, out var next) && next.InTargets(carrier.Coordinates))
        {
            carrier.OutputCarrier = next;
            return;
        }

        if (carrier.LinksBuildings && TryInventories(target, out var block, out var inventories))
        {
            carrier.OutputBuilding = block;
            carrier.OutputInventories = inventories;
        }
    }

    void LinkInput(BeltCarrier carrier)
    {
        if (!carrier.LinksBuildings)
        {
            return;
        }

        var target = carrier.InPort.Target;
        if (carriers.TryGetValue(target, out var previous) && previous.OutTargets(carrier.Coordinates))
        {
            return;
        }

        if (TryInventories(target, out var block, out var inventories))
        {
            carrier.InputBuilding = block;
            carrier.InputInventories = inventories;
        }
    }

    bool TryInventories(Vector3Int cell, out BlockObject block, out Inventories inventories)
    {
        block = null!;
        inventories = null!;
        foreach (var obj in blocks.GetObjectsAt(cell))
        {
            if (!obj || !obj.IsFinished)
            {
                continue;
            }

            if (obj.GetComponentOrNull<BeltCarrier>() || obj.GetComponentOrNull<BeltTeleporter>())
            {
                continue;
            }

            if (obj.GetComponentOrNull<Inventories>() is not { } found)
            {
                continue;
            }

            block = obj;
            inventories = found;
            return true;
        }

        return false;
    }

    void BuildOrder()
    {
        var count = carrierList.Count;
        moveOrder.Clear();
        indegree.Clear();
        queue.Clear();
        indexOf.Clear();
        while (upstreams.Count < count)
        {
            upstreams.Add([]);
        }

        for (var i = 0; i < count; i++)
        {
            upstreams[i].Clear();
            indegree.Add(0);
            indexOf[carrierList[i]] = i;
        }

        for (var i = 0; i < count; i++)
        {
            var next = carrierList[i].OutputCarrier;
            if (next is null || !indexOf.TryGetValue(next, out var downstream) || downstream == i)
            {
                continue;
            }

            upstreams[downstream].Add(i);
            indegree[i]++;
        }

        for (var i = 0; i < count; i++)
        {
            if (indegree[i] == 0)
            {
                queue.Add(i);
            }
        }

        var seen = new bool[count];
        var read = 0;
        while (read < queue.Count)
        {
            var index = queue[read++];
            seen[index] = true;
            moveOrder.Add(carrierList[index]);
            foreach (var upstream in upstreams[index])
            {
                indegree[upstream]--;
                if (indegree[upstream] == 0)
                {
                    queue.Add(upstream);
                }
            }
        }

        if (moveOrder.Count == count)
        {
            return;
        }

        for (var i = 0; i < count; i++)
        {
            if (!seen[i])
            {
                moveOrder.Add(carrierList[i]);
            }
        }
    }
}
