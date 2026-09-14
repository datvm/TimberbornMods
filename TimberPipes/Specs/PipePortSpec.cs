namespace TimberPipes.Specs;

public record PipePortSpec
{
    [Serialize]
    public Vector3Int Coordinates { get; init; }

    [Serialize]
    public Directions3D Directions { get; init; }

    [Serialize]
    public PipePortState State { get; init; }

}

public record DirectedPipeSpec : ComponentSpec;

public record ValvePipeSpec : ComponentSpec;

public record DischargePipeSpec : ComponentSpec
{
    // Same meaning as WaterOutputSpec.DistanceToGroundOffset. 0 = stop one tile under (Discharge).
    [Serialize]
    public float DistanceToGroundOffset { get; init; }

    // Headroom below the limit before dumping resumes (stops tiny on/off dumps).
    [Serialize]
    public float EjectBuffer { get; init; } = 0.1f;
}
