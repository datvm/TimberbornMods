namespace TimberPipes.Specs;

public record BuildingPipeSpec : ComponentSpec
{

    [Serialize]
    public ImmutableArray<PipePortSpec> Ports { get; init; } = [];

}
