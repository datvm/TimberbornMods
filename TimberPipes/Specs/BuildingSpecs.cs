namespace TimberPipes.Specs;

public record TransportPipeSpec : ComponentSpec;

public record PipeHeadliftSpec : ComponentSpec
{
    [Serialize]
    public float MaxHeadLift { get; init; }

    [Serialize]
    public int? InjectRate { get; init; }
}
