namespace TimberPipes.Specs;

public record PipeSimulationSpec : ComponentSpec
{
    [Serialize]
    public int Substeps { get; init; } = 4;

    [Serialize]
    public int DefaultInjectRate { get; init; } = 1;

    [Serialize]
    public int DefaultSlurpRate { get; init; } = 1;
}
