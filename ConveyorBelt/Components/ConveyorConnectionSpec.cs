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
    // Local coordinates + faces. In = belt can drop into this building, Out = belt can grab.
    // Missing spec (not this empty list) means any unoccupied occupancy face, like vanilla buildings.
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
