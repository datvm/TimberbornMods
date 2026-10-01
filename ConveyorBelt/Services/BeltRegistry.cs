namespace ConveyorBelt.Services;

[BindSingleton]
public class BeltRegistry(BeltLinks links, EventBus eventBus) : ILoadableSingleton
{
    readonly Dictionary<Vector3Int, BeltCarrier> carriers = [];
    readonly Dictionary<BeltCarrier, int> indexOf = [];
    readonly List<BeltCarrier> carrierList = [];
    readonly List<BeltMerger> mergers = [];
    readonly List<BeltCarrier> moveOrder = [];
    readonly List<BeltCarrier> downstream = [];
    readonly List<int> indegree = [];
    readonly List<int> queue = [];
    readonly List<List<int>> upstreams = [];
    bool dirty = true;

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

    public void Register(BeltMerger merger)
    {
        if (!mergers.Contains(merger))
        {
            mergers.Add(merger);
            dirty = true;
        }
    }

    public void Unregister(BeltMerger merger)
    {
        mergers.Remove(merger);
        dirty = true;
    }

    public IReadOnlyList<BeltCarrier> OrderForTick()
    {
        Ensure();
        for (var i = 0; i < mergers.Count; i++)
        {
            mergers[i].Rotate(moveOrder);
        }

        return moveOrder;
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
        for (var i = 0; i < mergers.Count; i++)
        {
            mergers[i].ClearInputs();
        }

        foreach (var carrier in carrierList)
        {
            carrier.Output = links.Resolve(new(carrier.Coordinates, carrier.OutPort.Target));
            carrier.Input = links.Resolve(new(carrier.Coordinates, carrier.InPort.Target));
        }

        BuildOrder();
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
            downstream.Clear();
            carrierList[i].Output?.CollectDownstream(downstream);
            foreach (var next in downstream)
            {
                if (!indexOf.TryGetValue(next, out var index) || index == i)
                {
                    continue;
                }

                upstreams[index].Add(i);
                indegree[i]++;
            }
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
