namespace ConveyorBelt.Components;

[AddTemplateModule2(typeof(BeltMergerSpec))]
public class BeltMerger(BeltLinks links, BeltRegistry registry) : BeltJunction
{
    static readonly Direction3D[] Entries = [Direction3D.Up, Direction3D.Left, Direction3D.Right];

    readonly List<Vector3Int> inputs = [];
    readonly List<JunctionHop> hops = [];
    float tickHours;

    public IReadOnlyList<JunctionHop> Hops => hops;

    public float HopHours => tickHours * 0.4f;

    protected override void FillPorts(List<LocalPort> localInputs, List<LocalPort> localOutputs) => BeltLayout.FillMerger(localInputs, localOutputs);

    protected override IBeltConnectionProvider CreateProvider() => new MergerProvider(this, links);

    protected override void Entered() => registry.Register(this);

    protected override void Exited() => registry.Unregister(this);

    public void RememberInput(Vector3Int from)
    {
        if (!inputs.Contains(from))
        {
            inputs.Add(from);
        }
    }

    public void ClearInputs() => inputs.Clear();

    public void BeginTick(float hoursPerTick)
    {
        tickHours = hoursPerTick;
        hops.Clear();
    }

    public Direction3D EntryDirection(Vector3Int from)
    {
        for (var i = 0; i < InputCells.Length && i < Entries.Length; i++)
        {
            if (InputCells[i] == from)
            {
                return Entries[i];
            }
        }

        return Direction3D.Up;
    }

    public void RememberHop(string id, Direction3D from, float spentHours, float usedHours)
    {
        if (usedHours <= 0f)
        {
            return;
        }

        hops.Add(new(
            id,
            from,
            BeltTrail.Portion(spentHours, tickHours),
            BeltTrail.Portion(spentHours + usedHours, tickHours)));
    }

    public void Rotate(List<BeltCarrier> order)
    {
        if (inputs.Count < 2)
        {
            return;
        }

        List<int> indices = [];
        for (var i = 0; i < order.Count; i++)
        {
            if (inputs.Contains(order[i].Coordinates))
            {
                indices.Add(i);
            }
        }

        if (indices.Count < 2)
        {
            return;
        }

        var first = order[indices[0]];
        for (var i = 0; i < indices.Count - 1; i++)
        {
            order[indices[i]] = order[indices[i + 1]];
        }

        order[indices[^1]] = first;
    }
}
