namespace ConveyorBelt.Components;

public readonly record struct BeltPort(Vector3Int Coordinates, Direction3D Direction, BeltPortKind Kind)
{
    public Vector3Int Target => Coordinates + Direction.ToOffset();

    public bool Faces(Vector3Int from) => Target == from;

    public bool Allows(BeltPortKind kind) => kind != BeltPortKind.None && Kind.HasFlag(kind);
}
