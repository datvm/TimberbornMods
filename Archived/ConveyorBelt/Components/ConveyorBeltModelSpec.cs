namespace ConveyorBelt.Components;

public record ConveyorBeltModelSpec : ComponentSpec
{
    [Serialize]
    public Vector3Int ArrowDirection { get; init; }
}