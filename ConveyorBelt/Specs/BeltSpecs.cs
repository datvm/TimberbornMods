namespace ConveyorBelt.Specs;

public enum BeltShape
{
    Straight,
    Corner,
    RiserUp,
    RiserDown,
    Impermeable,
}

public enum TeleporterKind
{
    Merger,
    Splitter,
    SmartSplitter,
    LiftUp,
    LiftDown,
}

public static class BeltShapeInfo
{
    public static bool LinksBuildings(BeltShape shape)
        => shape is BeltShape.Straight or BeltShape.Impermeable;
}

public record BeltCarrierSpec : ComponentSpec
{
    [Serialize]
    public BeltShape Shape { get; init; }

    [Serialize]
    public string Speed { get; init; } = "";
}

public record BeltTeleporterSpec : ComponentSpec
{
    [Serialize]
    public TeleporterKind Kind { get; init; }
}
