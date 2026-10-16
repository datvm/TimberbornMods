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

public record DirectedPipeSpec : ComponentSpec
{
    // Flip mirrors the mesh. When set, port flow stays on the same arm instead of reversing.
    [Serialize]
    public bool KeepFlowWhenFlipped { get; init; }
}

public record ValvePipeSpec : ComponentSpec;

public record ExtractionPipeSpec : ComponentSpec;

public record OutflowCounterSpec : ComponentSpec;

public record FlowLimitPipeSpec : ComponentSpec;

public record DischargePipeSpec : ComponentSpec
{
    // Same meaning as WaterOutputSpec.DistanceToGroundOffset. 0 = stop one tile under (Discharge).
    [Serialize]
    public float DistanceToGroundOffset { get; init; }

    // Headroom below the limit before dumping resumes (stops tiny on/off dumps).
    [Serialize]
    public float EjectBuffer { get; init; } = 0.1f;
}
