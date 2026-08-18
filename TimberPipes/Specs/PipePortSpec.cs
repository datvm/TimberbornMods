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
