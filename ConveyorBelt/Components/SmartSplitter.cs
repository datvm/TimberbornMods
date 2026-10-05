namespace ConveyorBelt.Components;

[AddTemplateModule2(typeof(SmartSplitterSpec))]
public class SmartSplitter : BeltSplitter
{
    static readonly ComponentKey SaveKey = new(nameof(SmartSplitter));
    static readonly PropertyKey<int> CursorKey = new("Cursor");
    static readonly ListKey<string> PortsKey = new("Ports");

    readonly IGoodService goods;

    public SmartSplitter(BeltRegistry registry, IGoodService goods) : base(registry)
    {
        this.goods = goods;
        var defaults = SmartSplitPort.SmartDefaults;
        for (var i = 0; i < defaults.Count; i++)
        {
            Sim.SetPort(i, defaults[i]);
        }
    }

    public void SetSplitPort(int index, SmartSplitPort port) => Sim.SetPort(index, port);

    public override void Save(IEntitySaver saver)
    {
        var s = saver.GetComponent(SaveKey);
        s.Set(CursorKey, Cursor);
        List<string> texts = [];
        foreach (var port in Sim.Ports)
        {
            texts.Add(port.Serialize());
        }

        s.Set(PortsKey, texts);
    }

    public override void Load(IEntityLoader loader)
    {
        if (!loader.TryGetComponent(SaveKey, out var s))
        {
            return;
        }

        if (s.Has(CursorKey))
        {
            Cursor = s.Get(CursorKey);
        }

        if (!s.Has(PortsKey))
        {
            return;
        }

        var texts = s.Get(PortsKey);
        for (var i = 0; i < texts.Count && i < Sim.Ports.Count; i++)
        {
            Sim.SetPort(i, SmartSplitPort.Deserialize(texts[i]).DropMissing(goods.HasGood));
        }
    }
}
