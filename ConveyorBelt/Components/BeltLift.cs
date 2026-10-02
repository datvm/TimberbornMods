namespace ConveyorBelt.Components;

[AddTemplateModule2(typeof(BeltLiftSpec))]
public class BeltLift(BeltLinks links, BeltRegistry registry) : BeltJunction, IBeltSplitter, IPersistentEntity
{
    static readonly ComponentKey SaveKey = new(nameof(BeltLift));
    static readonly PropertyKey<bool> SendingOutKey = new("SendingOut");
    static readonly SmartSplitPort[] AnyPorts = [SmartSplitPort.AnyPort, SmartSplitPort.AnyPort];

    readonly List<Vector3Int> inputs = [];

    public bool SendingOut { get; private set; }

    public int Cursor { get; set; }

    public IReadOnlyList<SmartSplitPort> SplitPorts => AnyPorts;

    public IBeltTarget Present(int output, IBeltTarget next) => next;

    protected override void FillPorts(List<LocalPort> localInputs, List<LocalPort> localOutputs) => BeltLayout.FillLift(SendingOut, localInputs, localOutputs);

    protected override IBeltConnectionProvider CreateProvider() => new LiftProvider(this, links);

    protected override void Entered() => registry.Register(this);

    protected override void Exited() => registry.Unregister(this);

    public void Toggle()
    {
        SendingOut = !SendingOut;
        RebuildSides();
        ClearInputs();
        registry.Invalidate();
        GetComponent<BeltArrows>()?.Rebuild();
    }

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
        if (!SendingOut || inputs.Count < 2)
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

    public void Save(IEntitySaver saver) => saver.GetComponent(SaveKey).Set(SendingOutKey, SendingOut);

    public void Load(IEntityLoader loader)
    {
        if (!loader.TryGetComponent(SaveKey, out var s))
        {
            return;
        }

        if (s.Has(SendingOutKey))
        {
            SendingOut = s.Get(SendingOutKey);
        }
    }
}
