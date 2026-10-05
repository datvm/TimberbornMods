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

public record FloodableBeltSpec : ComponentSpec;

public record BeltCarrierSpec : ComponentSpec
{
    [Serialize]
    public BeltShape Shape { get; init; }

    [Serialize]
    public string Speed { get; init; } = "";
}

public record BeltLiftSpec : ComponentSpec
{
    [Serialize]
    public bool Out { get; init; }
}

public record BeltMergerSpec : ComponentSpec;

public record BeltSplitterSpec : ComponentSpec;

public record SmartSplitterSpec : ComponentSpec;
