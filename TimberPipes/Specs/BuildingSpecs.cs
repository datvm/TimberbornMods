namespace TimberPipes.Specs;

public record TransportPipeSpec : ComponentSpec;

public record PipeToBuildingSpec : ComponentSpec
{
    [Serialize]
    public int? SlurpRate { get; init; }
}

public record BuildingToPipeSpec : ComponentSpec
{
    [Serialize]
    public float MaxHeadLift { get; init; }

    [Serialize]
    public int? InjectRate { get; init; }
}

public record FluidBufferBuildingSpec : ComponentSpec
{
    [Serialize]
    public int? Height { get; init; }
}
