namespace ConveyorBelt.Models;

public readonly record struct BeltEnd(Vector3Int Cell, Direction3D Direction)
{
    public Vector3Int Target => Cell + Direction.ToOffset();
}
