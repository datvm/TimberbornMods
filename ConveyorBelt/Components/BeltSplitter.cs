namespace ConveyorBelt.Components;

[AddTemplateModule2(typeof(BeltSplitterSpec))]
public class BeltSplitter(BeltLinks links) : BeltJunction, IBeltSplitter, IPersistentEntity
{
    static readonly ComponentKey SaveKey = new(nameof(BeltSplitter));
    static readonly PropertyKey<int> CursorKey = new("Cursor");
    static readonly SmartSplitPort[] AnyPorts = [SmartSplitPort.AnyPort, SmartSplitPort.AnyPort, SmartSplitPort.AnyPort];

    public int Cursor { get; set; }

    public virtual IReadOnlyList<SmartSplitPort> SplitPorts => AnyPorts;

    protected override void FillPorts(List<LocalPort> localInputs, List<LocalPort> localOutputs) => BeltLayout.FillSplitter(localInputs, localOutputs);

    protected override IBeltConnectionProvider CreateProvider() => new SplitterProvider(this, links);

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
