namespace ConveyorBelt.Algorithm;

// Advances every belt once, the same way the game does on Tick.
public sealed class BeltSimulation
{
    readonly List<SimBelt> belts = [];
    readonly List<SimMerger> mergers = [];
    readonly List<SimSplitter> splitters = [];
    readonly List<SimLift> lifts = [];
    readonly Dictionary<SimBelt, int> indexOf = [];
    readonly List<SimBelt> moveOrder = [];
    readonly List<SimBelt> downstream = [];
    readonly List<int> indegree = [];
    readonly List<int> queue = [];
    readonly List<List<int>> upstreams = [];
    int generation = 1;
    bool invalid = true;

    public int Generation => generation;

    public void Add(SimBelt belt)
    {
        if (belts.Contains(belt))
        {
            return;
        }

        belts.Add(belt);
        Invalidate();
    }

    public void Remove(SimBelt belt)
    {
        if (belts.Remove(belt))
        {
            Invalidate();
        }
    }

    public void Add(SimMerger merger) => AddTracked(mergers, merger);

    public void Remove(SimMerger merger) => RemoveTracked(mergers, merger);

    public void Add(SimSplitter splitter)
    {
        if (splitters.Contains(splitter))
        {
            return;
        }

        splitters.Add(splitter);
    }

    public void Remove(SimSplitter splitter) => splitters.Remove(splitter);

    public void Add(SimLift lift) => AddTracked(lifts, lift);

    public void Remove(SimLift lift) => RemoveTracked(lifts, lift);

    public void Invalidate() => invalid = true;

    void AddTracked<T>(List<T> list, T item)
    {
        if (list.Contains(item))
        {
            return;
        }

        list.Add(item);
        Invalidate();
    }

    void RemoveTracked<T>(List<T> list, T item)
    {
        if (list.Remove(item))
        {
            Invalidate();
        }
    }

    public void Tick(float hoursPerTick)
    {
        EnsureOrder();
        ShareTurns();

        generation++;
        if (generation == 0)
        {
            generation = 1;
        }

        for (var i = 0; i < moveOrder.Count; i++)
        {
            moveOrder[i].BeginTick(generation);
        }

        for (var i = 0; i < moveOrder.Count; i++)
        {
            moveOrder[i].Move(hoursPerTick);
        }

        for (var i = 0; i < moveOrder.Count; i++)
        {
            moveOrder[i].Pull(hoursPerTick);
        }
    }

    // When two junctions list the same feeder belts, only the one that sees
    // every feeder rotates them. A belt between junctions keeps each turn local.
    void ShareTurns()
    {
        for (var i = 0; i < mergers.Count; i++)
        {
            Turn(mergers[i]);
        }

        for (var i = 0; i < lifts.Count; i++)
        {
            Turn(lifts[i]);
        }
    }

    void Turn(IInputTurn junction)
    {
        if (HeldByAnother(junction, junction.Inputs))
        {
            return;
        }

        junction.Rotate(moveOrder);
    }

    bool HeldByAnother(object self, IReadOnlyList<SimBelt> mine)
    {
        if (mine.Count < 2)
        {
            return true;
        }

        for (var i = 0; i < mergers.Count; i++)
        {
            if (Covers(mergers[i], mergers[i].Inputs, mine, self))
            {
                return true;
            }
        }

        for (var i = 0; i < lifts.Count; i++)
        {
            if (Covers(lifts[i], lifts[i].Inputs, mine, self))
            {
                return true;
            }
        }

        return false;
    }

    bool Covers(object other, IReadOnlyList<SimBelt> theirs, IReadOnlyList<SimBelt> mine, object self)
    {
        if (ReferenceEquals(other, self) || theirs.Count < mine.Count || !ContainsAll(theirs, mine))
        {
            return false;
        }

        if (theirs.Count > mine.Count)
        {
            return true;
        }

        return Key(other) < Key(self);
    }

    static bool ContainsAll(IReadOnlyList<SimBelt> theirs, IReadOnlyList<SimBelt> mine)
    {
        for (var i = 0; i < mine.Count; i++)
        {
            if (!theirs.Contains(mine[i]))
            {
                return false;
            }
        }

        return true;
    }

    int Key(object junction)
    {
        if (junction is SimMerger merger)
        {
            return mergers.IndexOf(merger);
        }

        return junction is SimLift lift ? mergers.Count + lifts.IndexOf(lift) : 0;
    }

    void EnsureOrder()
    {
        if (!invalid)
        {
            return;
        }

        invalid = false;
        BuildOrder();
    }

    void BuildOrder()
    {
        var count = belts.Count;
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
            indexOf[belts[i]] = i;
        }

        for (var i = 0; i < count; i++)
        {
            downstream.Clear();
            belts[i].Output?.CollectDownstream(downstream);
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
            moveOrder.Add(belts[index]);
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
                moveOrder.Add(belts[i]);
            }
        }
    }
}
