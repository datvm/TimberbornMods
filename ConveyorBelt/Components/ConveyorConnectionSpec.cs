namespace ConveyorBelt.Components;

[Flags]
public enum BeltPortKind
{
    None = 0,
    In = 1,
    Out = 2,
    Both = In | Out,
}

public record ConveyorConnectionSpec : ComponentSpec
{
    [Serialize]
    public ImmutableArray<BeltPortSpec> Ports { get; init; } = [];
}

public record BeltPortSpec
{
    [Serialize]
    public Vector3Int Coordinates { get; init; }

    [Serialize]
    public Directions3D Directions { get; init; }

    [Serialize]
    public BeltPortKind Kind { get; init; } = BeltPortKind.Both;
}
