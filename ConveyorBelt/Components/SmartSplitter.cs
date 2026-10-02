namespace ConveyorBelt.Components;

[AddTemplateModule2(typeof(SmartSplitterSpec))]
public class SmartSplitter(BeltLinks links, BeltRegistry registry) : BeltSplitter(links, registry)
{
    static readonly ComponentKey SaveKey = new(nameof(SmartSplitter));
    static readonly PropertyKey<int> CursorKey = new("Cursor");
    static readonly ListKey<string> PortsKey = new("Ports");

    readonly List<SmartSplitPort> ports = [SmartSplitPort.AnyPort, SmartSplitPort.AnyPort, SmartSplitPort.AnyPort];

    public override IReadOnlyList<SmartSplitPort> SplitPorts => ports;

    public void SetSplitPort(int index, SmartSplitPort port)
    {
        if (index < 0 || index >= ports.Count)
        {
            return;
        }

        ports[index] = port;
    }

    public override void Save(IEntitySaver saver)
    {
        var s = saver.GetComponent(SaveKey);
        s.Set(CursorKey, Cursor);
        List<string> texts = [];
        foreach (var port in ports)
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
        for (var i = 0; i < texts.Count && i < ports.Count; i++)
        {
            ports[i] = SmartSplitPort.Deserialize(texts[i]);
        }
    }
}
