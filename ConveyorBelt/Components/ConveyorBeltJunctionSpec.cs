namespace ConveyorBelt.Components;

public record ConveyorBeltJunctionSpec : ComponentSpec
{
    [Serialize]
    public ImmutableArray<Vector3Int> InputCoordinates { get; init; } = [];

    [Serialize]
    public Vector3Int OutputCoordinates { get; init; }
}
