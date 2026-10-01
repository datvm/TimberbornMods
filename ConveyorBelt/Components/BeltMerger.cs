namespace ConveyorBelt.Components;

[AddTemplateModule2(typeof(BeltMergerSpec))]
public class BeltMerger(BeltLinks links, BeltRegistry registry) : BeltJunction
{
    readonly List<Vector3Int> inputs = [];

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
