namespace ConveyorBelt.Components;

[AddTemplateModule2(typeof(BeltSplitterSpec))]
public class BeltSplitter(BeltRegistry registry) : BeltJunction, IPersistentEntity
{
    static readonly ComponentKey SaveKey = new(nameof(BeltSplitter));
    static readonly PropertyKey<int> CursorKey = new("Cursor");

    public SimSplitter Sim { get; } = new([SmartSplitPort.AnyPort, SmartSplitPort.AnyPort, SmartSplitPort.AnyPort]);

    public int Cursor
    {
        get => Sim.Cursor;
        set => Sim.Cursor = value;
    }

    public virtual IReadOnlyList<SmartSplitPort> SplitPorts => Sim.Ports;

    protected override void FillPorts(List<BeltPort> localInputs, List<BeltPort> localOutputs)
        => BeltLayout.FillSplitter(localInputs, localOutputs);

    public override bool TryProvide(BeltApproach approach, SimBelt? upstream, out ISimLink link)
    {
        link = SimLinks.None;
        if (upstream is not { Plain: true } || !AcceptsInput(approach.From))
        {
            return false;
        }

        link = SimLinks.ToBelt(Sim.Belt);
        return true;
    }

    protected override void Entered() => registry.Register(this);

    protected override void Exited() => registry.Unregister(this);

    public virtual void Save(IEntitySaver saver) => saver.GetComponent(SaveKey).Set(CursorKey, Cursor);

    public virtual void Load(IEntityLoader loader)
    {
        if (!loader.TryGetComponent(SaveKey, out var s))
        {
            return;
        }

        if (s.Has(CursorKey))
        {
            Cursor = s.Get(CursorKey);
        }
    }
}
