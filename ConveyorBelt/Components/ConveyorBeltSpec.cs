namespace ConveyorBelt.Components;

public record ConveyorBeltSpec : ComponentSpec
{
    [Serialize]
    public Vector3Int InputCoordinates { get; init; }

    [Serialize]
    public Vector3Int OutputCoordinates { get; init; }

    [Serialize]
    public Vector3Int ArrowDirection { get; init; }

    [Serialize]
    public ImmutableArray<string> ForbiddenGoodTypes { get; init; } = [];

    [Serialize]
    public float TravelTimeHours { get; init; }

    [Serialize]
    public int Capacity { get; init; }

}
