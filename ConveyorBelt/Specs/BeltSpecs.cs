namespace ConveyorBelt.Specs;

public enum BeltShape
{
    Straight,
    Corner,
    RiserUp,
    RiserDown,
    Impermeable,
}

public record BeltSystemSpec : ComponentSpec;

public record BeltCarrierSpec : ComponentSpec
{
    [Serialize]
    public BeltShape Shape { get; init; }

    [Serialize]
    public string Speed { get; init; } = "";
}

public record BeltLiftSpec : ComponentSpec;

public record BeltMergerSpec : ComponentSpec;

public record BeltSplitterSpec : ComponentSpec;

public record SmartSplitterSpec : ComponentSpec;
