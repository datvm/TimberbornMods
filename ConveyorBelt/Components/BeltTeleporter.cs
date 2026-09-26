namespace ConveyorBelt.Components;

[AddTemplateModule2(typeof(BeltTeleporterSpec))]
public class BeltTeleporter(BeltRegistry registry) : BaseComponent, IAwakableComponent, IInitializableEntity, IFinishedStateListener, IPersistentEntity
{
    static readonly ComponentKey SaveKey = new(nameof(BeltTeleporter));
    static readonly PropertyKey<int> CursorKey = new("Cursor");
    static readonly ListKey<string> PortsKey = new("Ports");

    readonly List<LocalPort> localInputs = [];
    readonly List<LocalPort> localOutputs = [];
    readonly List<SmartSplitPort> splitPorts = [];

    BlockObject block = null!;
    MechanicalNode mechanical = null!;
    Vector3Int[] inputCells = [];
    Vector3Int[] outputCells = [];

    public TeleporterKind Kind { get; private set; }
    public Vector3Int Coordinates => block.Coordinates;
    public bool Running => !mechanical.IsConsumer || mechanical.PowerEfficiency >= 1f;
    public int Cursor { get; private set; }
    public IReadOnlyList<SmartSplitPort> SplitPorts => splitPorts;

    public void Awake()
    {
        block = GetComponent<BlockObject>();
        mechanical = GetComponent<MechanicalNode>();
        Kind = GetComponent<BeltTeleporterSpec>().Kind;
        if (Kind is TeleporterKind.Splitter or TeleporterKind.SmartSplitter)
        {
            splitPorts.Add(SmartSplitPort.AnyPort);
            splitPorts.Add(SmartSplitPort.AnyPort);
            splitPorts.Add(SmartSplitPort.AnyPort);
        }
    }

    public void InitializeEntity() => RebuildSides();

    public void OnEnterFinishedState()
    {
        RebuildSides();
        registry.Register(this);
    }

    public void OnExitFinishedState() => registry.Unregister(this);

    public void SetSplitPort(int index, SmartSplitPort port)
    {
        if (index < 0 || index >= splitPorts.Count)
        {
            return;
        }

        splitPorts[index] = port;
    }

    public void Teleport()
    {
        if (!Running)
        {
            return;
        }

        if (Kind is TeleporterKind.LiftUp or TeleporterKind.LiftDown)
        {
            TeleportAcrossSides();
            return;
        }

        if (Kind is TeleporterKind.Splitter or TeleporterKind.SmartSplitter)
        {
            TeleportSplit();
            return;
        }

        TeleportMerge();
    }

    public void Save(IEntitySaver saver)
    {
        var s = saver.GetComponent(SaveKey);
        s.Set(CursorKey, Cursor);
        if (splitPorts.Count == 0)
        {
            return;
        }

        List<string> texts = [];
        foreach (var port in splitPorts)
        {
            texts.Add(port.Serialize());
        }

        s.Set(PortsKey, texts);
    }

    public void Load(IEntityLoader loader)
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
        for (var i = 0; i < texts.Count && i < splitPorts.Count; i++)
        {
            splitPorts[i] = SmartSplitPort.Deserialize(texts[i]);
        }
    }

    void TeleportMerge()
    {
        var count = inputCells.Length;
        if (count == 0 || outputCells.Length == 0)
        {
            return;
        }

        var cursor = Cursor;
        for (var n = 0; n < count; n++)
        {
            var index = (cursor + n) % count;
            if (!TryGiver(inputCells[index], out var giver, out var goodId))
            {
                continue;
            }

            if (!TryReceiver(outputCells[0], goodId, out var receiver))
            {
                continue;
            }

            if (!giver.TryPop(out goodId))
            {
                continue;
            }

            receiver.Push(goodId);
            Cursor = (index + 1) % count;
            return;
        }
    }

    void TeleportSplit()
    {
        if (inputCells.Length == 0)
        {
            return;
        }

        if (!TryGiver(inputCells[0], out var giver, out var goodId))
        {
            return;
        }

        var cursor = Cursor;
        var index = SmartSplitPicker.Pick(splitPorts, goodId, i => CanTakeAt(i, goodId), ref cursor);
        if (index < 0 || !TryReceiver(outputCells[index], goodId, out var receiver) || !giver.TryPop(out goodId))
        {
            return;
        }

        receiver.Push(goodId);
        Cursor = cursor;
    }

    bool CanTakeAt(int index, string goodId)
        => index >= 0
            && index < outputCells.Length
            && registry.ReceiverAt(outputCells[index], Coordinates) is { } receiver
            && receiver.CanTake(goodId);

    void TeleportAcrossSides()
    {
        if (inputCells.Length == 0)
        {
            return;
        }

        var cursor = Cursor;
        var count = inputCells.Length;
        for (var n = 0; n < count; n++)
        {
            var index = (cursor + n) % count;
            if (!TryGiver(inputCells[index], out var giver, out var goodId))
            {
                continue;
            }

            for (var o = 0; o < outputCells.Length; o++)
            {
                if (outputCells[o] == inputCells[index])
                {
                    continue;
                }

                if (!TryReceiver(outputCells[o], goodId, out var receiver) || !giver.TryPop(out goodId))
                {
                    continue;
                }

                receiver.Push(goodId);
                Cursor = (index + 1) % count;
                return;
            }
        }
    }

    bool TryGiver(Vector3Int cell, out BeltCarrier giver, out string goodId)
    {
        goodId = "";
        giver = registry.GiverAt(cell, Coordinates)!;
        if (!giver || !giver.HeadReady)
        {
            return false;
        }

        goodId = giver.Items[0].Id;
        return true;
    }

    bool TryReceiver(Vector3Int cell, string goodId, out BeltCarrier receiver)
    {
        receiver = registry.ReceiverAt(cell, Coordinates)!;
        return receiver && receiver.CanTake(goodId);
    }

    void RebuildSides()
    {
        BeltLayout.FillTeleporter(Kind, localInputs, localOutputs);
        inputCells = Cells(localInputs);
        outputCells = Cells(localOutputs);
    }

    Vector3Int[] Cells(List<LocalPort> ports)
    {
        var cells = new Vector3Int[ports.Count];
        for (var i = 0; i < ports.Count; i++)
        {
            cells[i] = block.Coordinates + block.TransformDirection(ports[i].Direction).ToOffset();
        }

        return cells;
    }
}
