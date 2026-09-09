namespace TimberPipes.Specs;

public record PipeSimulationSpec : ComponentSpec
{
    [Serialize]
    public float EqualizeK { get; init; } = 0.8f;

    [Serialize]
    public int Substeps { get; init; } = 4;

    [Serialize]
    public int DefaultInjectRate { get; init; } = 1;

    [Serialize]
    public int DefaultSlurpRate { get; init; } = 1;
}
