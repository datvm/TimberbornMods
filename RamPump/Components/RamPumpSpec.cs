namespace RamPump.Components;

public record RamPumpSpec : ComponentSpec
{
    [Serialize]
    public float PumpPortion { get; init; }

    [Serialize]
    public float Buffer { get; init; }

    [Serialize]
    public Vector3Int InputChamber { get; init; }

    [Serialize]
    public Vector3Int OutputCoordinates { get; init; }

    [Serialize]
    public ImmutableArray<Vector3Int> WaterBlockingCoordinates { get; init; }

}
