namespace TimberPipes.Specs;

public record BuildingPipeTargetPortsSpec : ComponentSpec
{
    // Local coordinates + faces. OpenIn = valve can fill, OpenOut = valve can extract.
    // Missing spec (not this empty list) means any occupied face, like vanilla buildings.
    [Serialize]
    public ImmutableArray<PipePortSpec> Ports { get; init; } = [];
}
