namespace ConveyorBelt.Algorithm;

public enum SimKind
{
    Straight,
    Corner,
    RiserUp,
    RiserDown,
    Impermeable,
    Merger,
    Splitter,
    SmartSplitter,
    LiftIn,
    LiftOut,
    Source,
    Sink,
}

public readonly record struct FlowPort(BeltDir Local, BeltDir World, BeltCell Target, bool Incoming);

public sealed class LayoutPiece
{
    readonly Dictionary<string, int> received = [];

    public int Id { get; init; }
    public SimKind Kind { get; init; }
    public BeltCell Cell { get; set; }
    public int QuarterTurns { get; set; }
    public float ItemsPerHour { get; set; } = 4f;
    public bool Running { get; set; } = true;
    public string GoodId { get; set; } = "Log";
    public bool Infinite { get; set; } = true;
    public int Stock { get; set; } = 20;
    public int Delivered { get; private set; }
    public string LastGood { get; private set; } = "";
    public SimBelt? Belt { get; internal set; }
    public SimMerger? Merger { get; internal set; }
    public SimSplitter? Splitter { get; internal set; }
    public SimLift? Lift { get; internal set; }
    internal ISimSource? Source { get; set; }
    internal ISimLink? Sink { get; set; }

    public IReadOnlyList<BeltGood> Items => Belt?.Items ?? [];
    public IReadOnlyList<BeltTrail> Trails => Belt?.Trails ?? [];

    public int CountOf(string goodId) => received.TryGetValue(goodId, out var count) ? count : 0;

    internal void ResetCounts()
    {
        Delivered = 0;
        LastGood = "";
        received.Clear();
    }

    internal void Accept(string goodId)
    {
        Delivered++;
        LastGood = goodId;
        received.TryGetValue(goodId, out var count);
        received[goodId] = count + 1;
    }
}

public sealed class LayoutBoard
{
    readonly BeltSimulation simulation = new();
    readonly List<LayoutPiece> pieces = [];
    readonly Dictionary<BeltCell, LayoutPiece> byCell = [];
    readonly List<BeltPort> inputs = [];
    readonly List<BeltPort> outputs = [];
    readonly HashSet<BeltCell> trail = [];
    int nextId = 1;
    bool dirty = true;

    public IReadOnlyList<LayoutPiece> Pieces => pieces;
    public int Generation => simulation.Generation;

    public LayoutPiece? At(BeltCell cell) => byCell.TryGetValue(cell, out var piece) ? piece : null;

    public LayoutPiece? Find(int id)
    {
        for (var i = 0; i < pieces.Count; i++)
        {
            if (pieces[i].Id == id)
            {
                return pieces[i];
            }
        }

        return null;
    }

    public LayoutPiece Place(SimKind kind, BeltCell cell, int quarterTurns, float itemsPerHour)
    {
        if (byCell.TryGetValue(cell, out var previous))
        {
            Remove(previous.Id);
        }

        var piece = Create(kind, cell, quarterTurns, itemsPerHour);
        pieces.Add(piece);
        byCell[cell] = piece;
        Attach(piece);
        dirty = true;
        return piece;
    }

    public void Remove(int id)
    {
        var piece = Find(id);
        if (piece is null)
        {
            return;
        }

        Detach(piece);
        pieces.Remove(piece);
        if (byCell.TryGetValue(piece.Cell, out var current) && current == piece)
        {
            byCell.Remove(piece.Cell);
        }

        dirty = true;
    }

    public void Rotate(int id)
    {
        var piece = Find(id);
        if (piece is null)
        {
            return;
        }

        piece.QuarterTurns = (piece.QuarterTurns + 1) % 4;
        dirty = true;
    }

    public void SetItemsPerHour(int id, float itemsPerHour)
    {
        var piece = Find(id);
        if (piece is null)
        {
            return;
        }

        piece.ItemsPerHour = itemsPerHour;
        if (piece.Belt is { Plain: true })
        {
            piece.Belt.ItemsPerHour = itemsPerHour;
        }
    }

    public void SetRunning(int id, bool running)
    {
        var piece = Find(id);
        if (piece is null)
        {
            return;
        }

        piece.Running = running;
        if (piece.Belt is not null)
        {
            piece.Belt.Running = running;
        }

        if (piece.Merger is not null)
        {
            piece.Merger.Running = running;
        }

        if (piece.Splitter is not null)
        {
            piece.Splitter.Running = running;
        }

        if (piece.Lift is not null)
        {
            piece.Lift.Running = running;
        }
    }

    public void SetSplitPort(int id, int index, SmartSplitPort port) => Find(id)?.Splitter?.SetPort(index, port);

    public void ClearGoods()
    {
        for (var i = 0; i < pieces.Count; i++)
        {
            var piece = pieces[i];
            piece.Belt?.Clear();
            piece.ResetCounts();
        }
    }

    public void Tick(float hoursPerTick)
    {
        if (dirty)
        {
            Relink();
        }

        simulation.Tick(hoursPerTick);
    }

    public IReadOnlyList<FlowPort> Ports(LayoutPiece piece) => WorldPorts(piece);

    public void Relink()
    {
        dirty = false;
        for (var i = 0; i < pieces.Count; i++)
        {
            var piece = pieces[i];
            piece.Merger?.ClearInputs();
            piece.Lift?.ClearInputs();
            if (piece.Belt is not null)
            {
                piece.Belt.Output = null;
                piece.Belt.Input = null;
            }
        }

        for (var i = 0; i < pieces.Count; i++)
        {
            var piece = pieces[i];
            if (piece.Belt is not { Plain: true })
            {
                continue;
            }

            var ports = WorldPorts(piece);
            for (var p = 0; p < ports.Count; p++)
            {
                var port = ports[p];
                if (port.Incoming)
                {
                    piece.Belt.Input = FindSource(port.Target);
                    continue;
                }

                piece.Belt.Output = Resolve(piece.Cell, port.Target, piece.Belt);
            }
        }

        for (var i = 0; i < pieces.Count; i++)
        {
            WireJunction(pieces[i]);
        }

        simulation.Invalidate();
    }

    LayoutPiece Create(SimKind kind, BeltCell cell, int quarterTurns, float itemsPerHour)
    {
        var piece = new LayoutPiece
        {
            Id = nextId++,
            Kind = kind,
            Cell = cell,
            QuarterTurns = ((quarterTurns % 4) + 4) % 4,
            ItemsPerHour = itemsPerHour,
        };

        switch (kind)
        {
            case SimKind.Merger:
                piece.Merger = new SimMerger();
                piece.Belt = piece.Merger.Belt;
                break;
            case SimKind.Splitter:
                piece.Splitter = new SimSplitter([SmartSplitPort.AnyPort, SmartSplitPort.AnyPort, SmartSplitPort.AnyPort]);
                piece.Belt = piece.Splitter.Belt;
                break;
            case SimKind.SmartSplitter:
                piece.Splitter = new SimSplitter(SmartSplitPort.SmartDefaults);
                piece.Belt = piece.Splitter.Belt;
                break;
            case SimKind.LiftIn:
                piece.Lift = new SimLift(false);
                piece.Belt = piece.Lift.Belt;
                break;
            case SimKind.LiftOut:
                piece.Lift = new SimLift(true);
                piece.Belt = piece.Lift.Belt;
                break;
            case SimKind.Source:
                piece.Source = new StockSource(piece);
                break;
            case SimKind.Sink:
                piece.Sink = new SinkLink(piece);
                break;
            default:
                piece.Belt = new SimBelt { ItemsPerHour = itemsPerHour, Running = true };
                break;
        }

        return piece;
    }

    void Attach(LayoutPiece piece)
    {
        if (piece.Belt is not null)
        {
            if (piece.Belt.Plain)
            {
                piece.Belt.ItemsPerHour = piece.ItemsPerHour;
            }

            piece.Belt.Running = piece.Running;
            simulation.Add(piece.Belt);
        }

        if (piece.Merger is not null)
        {
            simulation.Add(piece.Merger);
        }

        if (piece.Splitter is not null)
        {
            simulation.Add(piece.Splitter);
        }

        if (piece.Lift is not null)
        {
            simulation.Add(piece.Lift);
        }
    }

    void Detach(LayoutPiece piece)
    {
        if (piece.Belt is not null)
        {
            simulation.Remove(piece.Belt);
        }

        if (piece.Merger is not null)
        {
            simulation.Remove(piece.Merger);
        }

        if (piece.Splitter is not null)
        {
            simulation.Remove(piece.Splitter);
        }

        if (piece.Lift is not null)
        {
            simulation.Remove(piece.Lift);
        }
    }

    ISimSource? FindSource(BeltCell cell)
    {
        if (!byCell.TryGetValue(cell, out var piece))
        {
            return null;
        }

        return piece.Source;
    }

    ISimLink? Resolve(BeltCell from, BeltCell to, SimBelt? upstream)
    {
        var root = trail.Count == 0;
        if (!trail.Add(to))
        {
            return null;
        }

        try
        {
            return Bind(from, to, upstream);
        }
        finally
        {
            if (root)
            {
                trail.Clear();
            }
        }
    }

    ISimLink? Bind(BeltCell from, BeltCell to, SimBelt? upstream)
    {
        if (!byCell.TryGetValue(to, out var piece))
        {
            return null;
        }

        if (piece.Sink is not null)
        {
            return piece.Sink;
        }

        if (piece.Belt is not null)
        {
            if (!Faces(piece, from))
            {
                return null;
            }

            return SimLinks.ToBelt(piece.Belt);
        }

        if (piece.Merger is not null)
        {
            if (upstream is not { Plain: true } || !Faces(piece, from))
            {
                return null;
            }

            piece.Merger.Remember(upstream);
            return SimLinks.ToBelt(piece.Merger.Belt);
        }

        if (piece.Splitter is not null)
        {
            if (upstream is not { Plain: true } || !Faces(piece, from))
            {
                return null;
            }

            return SimLinks.ToBelt(piece.Splitter.Belt);
        }

        if (piece.Lift is not null)
        {
            if (upstream is not { Plain: true } || !Faces(piece, from))
            {
                return null;
            }

            if (piece.Lift.SendingOut)
            {
                piece.Lift.Remember(upstream);
            }

            return SimLinks.ToBelt(piece.Lift.Belt);
        }

        return null;
    }

    void WireJunction(LayoutPiece piece)
    {
        if (piece.Merger is { } merger)
        {
            merger.Belt.Output = FirstPlain(piece);
            return;
        }

        if (piece.Splitter is { } splitter)
        {
            splitter.Attach(PlainOutputs(piece));
            return;
        }

        if (piece.Lift is not { } lift)
        {
            return;
        }

        if (lift.SendingOut)
        {
            lift.AttachOut(FirstPlain(piece));
            return;
        }

        lift.AttachSplit(PlainOutputs(piece));
    }

    ISimLink FirstPlain(LayoutPiece piece)
    {
        var ports = Outputs(piece);
        if (ports.Count == 0)
        {
            return SimLinks.None;
        }

        return PlainLink(piece.Cell, ports[0].Target);
    }

    IReadOnlyList<ISimLink> PlainOutputs(LayoutPiece piece)
    {
        var ports = Outputs(piece);
        var links = new ISimLink[ports.Count];
        for (var i = 0; i < ports.Count; i++)
        {
            links[i] = PlainLink(piece.Cell, ports[i].Target);
        }

        return links;
    }

    ISimLink PlainLink(BeltCell from, BeltCell to)
    {
        if (!byCell.TryGetValue(to, out var piece) || piece.Belt is not { Plain: true } belt || !Faces(piece, from))
        {
            return SimLinks.None;
        }

        return SimLinks.ToBelt(belt);
    }

    bool Faces(LayoutPiece piece, BeltCell from)
    {
        var ports = WorldPorts(piece);
        for (var i = 0; i < ports.Count; i++)
        {
            if (ports[i].Incoming && ports[i].Target == from)
            {
                return true;
            }
        }

        return false;
    }

    List<FlowPort> Outputs(LayoutPiece piece)
    {
        var ports = WorldPorts(piece);
        List<FlowPort> found = [];
        for (var i = 0; i < ports.Count; i++)
        {
            if (!ports[i].Incoming)
            {
                found.Add(ports[i]);
            }
        }

        return found;
    }

    List<FlowPort> WorldPorts(LayoutPiece piece)
    {
        inputs.Clear();
        outputs.Clear();
        switch (piece.Kind)
        {
            case SimKind.Merger:
                BeltLayout.FillMerger(inputs, outputs);
                break;
            case SimKind.Splitter:
            case SimKind.SmartSplitter:
                BeltLayout.FillSplitter(inputs, outputs);
                break;
            case SimKind.LiftIn:
            case SimKind.LiftOut:
                BeltLayout.FillLift(piece.Kind == SimKind.LiftOut, inputs, outputs);
                break;
            case SimKind.Source:
            case SimKind.Sink:
                return [];
            default:
                BeltLayout.FillCarrier(Form(piece.Kind), inputs);
                break;
        }

        List<FlowPort> ports = [];
        Add(piece, inputs, ports);
        Add(piece, outputs, ports);
        return ports;
    }

    static void Add(LayoutPiece piece, List<BeltPort> local, List<FlowPort> into)
    {
        for (var i = 0; i < local.Count; i++)
        {
            var world = BeltDirs.Turn(local[i].Direction, piece.QuarterTurns);
            into.Add(new(local[i].Direction, world, piece.Cell.Step(world), local[i].Incoming));
        }
    }

    static BeltForm Form(SimKind kind) => kind switch
    {
        SimKind.Corner => BeltForm.Corner,
        SimKind.RiserUp => BeltForm.RiserUp,
        SimKind.RiserDown => BeltForm.RiserDown,
        SimKind.Impermeable => BeltForm.Impermeable,
        _ => BeltForm.Straight,
    };

    sealed class StockSource(LayoutPiece piece) : ISimSource
    {
        public bool TryTake(out string goodId)
        {
            goodId = "";
            if (piece.GoodId.Length == 0)
            {
                return false;
            }

            if (!piece.Infinite)
            {
                if (piece.Stock <= 0)
                {
                    return false;
                }

                piece.Stock--;
            }

            goodId = piece.GoodId;
            return true;
        }
    }

    sealed class SinkLink(LayoutPiece piece) : ISimLink, ISimTarget
    {
        public bool CanTarget(string goodId) => true;

        public bool TryGetTarget(string goodId, out ISimTarget target)
        {
            target = this;
            return true;
        }

        public bool CanAccept(string goodId) => true;

        public bool TryAccept(string goodId, float leftoverHours, float spentHours)
        {
            piece.Accept(goodId);
            return true;
        }

        public void Commit()
        {
        }

        public void CollectDownstream(List<SimBelt> into)
        {
        }
    }
}
