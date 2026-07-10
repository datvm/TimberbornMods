namespace ConveyorBelt.Components;

public record ConveyorBeltJunctionSpec : ComponentSpec
{
    [Serialize]
    public ImmutableArray<Vector3Int> InputCoordinates { get; init; } = [];

    [Serialize]
    public ImmutableArray<Vector3Int> OutputCoordinates { get; init; } = [];
}
