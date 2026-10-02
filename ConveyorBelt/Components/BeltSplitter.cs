namespace ConveyorBelt.Components;

[AddTemplateModule2(typeof(BeltSplitterSpec))]
public class BeltSplitter(BeltLinks links, BeltRegistry registry) : BeltJunction, IBeltSplitter, IPersistentEntity
{
    static readonly ComponentKey SaveKey = new(nameof(BeltSplitter));
    static readonly PropertyKey<int> CursorKey = new("Cursor");
    static readonly SmartSplitPort[] AnyPorts = [SmartSplitPort.AnyPort, SmartSplitPort.AnyPort, SmartSplitPort.AnyPort];

    readonly List<JunctionHop> hops = [];
    float tickHours;

    public int Cursor { get; set; }

    public IReadOnlyList<JunctionHop> Hops => hops;

    public float HopHours => tickHours * 0.4f;

    public virtual IReadOnlyList<SmartSplitPort> SplitPorts => AnyPorts;

    protected override void FillPorts(List<LocalPort> localInputs, List<LocalPort> localOutputs) => BeltLayout.FillSplitter(localInputs, localOutputs);

    protected override IBeltConnectionProvider CreateProvider() => new SplitterProvider(this, links);

    protected override void Entered() => registry.Register(this);

    protected override void Exited() => registry.Unregister(this);

    public void BeginTick(float hoursPerTick)
    {
        tickHours = hoursPerTick;
        hops.Clear();
    }

    public Direction3D ExitDirection(int index) => index switch
    {
        0 => Direction3D.Left,
        1 => Direction3D.Down,
        2 => Direction3D.Right,
        _ => Direction3D.Down,
    };

    public void RememberHop(string id, Direction3D to, float spentHours, float usedHours)
    {
        if (usedHours <= 0f)
        {
            return;
        }

        hops.Add(new(
            id,
            to,
            BeltTrail.Portion(spentHours, tickHours),
            BeltTrail.Portion(spentHours + usedHours, tickHours)));
    }

    public IBeltTarget Present(int output, IBeltTarget next) => new SplitterHopTarget(this, ExitDirection(output), next);

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
