namespace ConveyorBelt.Specs;

public record ConveyorBeltSettingsSpec : ComponentSpec
{
    [Serialize]
    public int Capacity { get; init; } = 5;

    [Serialize]
    public ImmutableArray<ConveyorBeltSpeedLevel> Levels { get; init; } = [];
}

public record ConveyorBeltSpeedLevel
{
    [Serialize]
    public string Id { get; init; } = null!;

    [Serialize]
    public float ItemsPerHour { get; init; }
}
