namespace TimberPipes.Specs;

public record TransportPipeSpec : ComponentSpec;
public record PipeToBuildingSpec : ComponentSpec;
public record BuildingToPipeSpec : ComponentSpec;

public record FluidBufferBuildingSpec : ComponentSpec
{
    [Serialize]
    public int? Height { get; init; }
}