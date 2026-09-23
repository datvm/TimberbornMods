namespace PowerOverdrive.Specs;

public record PowerOverdriveSpec : ComponentSpec
{
    [Serialize]
    public ImmutableArray<PowerOverdriveLevelSpec> Levels { get; init; } = [];
}

public record PowerOverdriveLevelSpec
{
    [Serialize]
    public string ItemId { get; init; } = null!;

    [Serialize]
    public float ProductionBonus { get; init; }

    [Serialize]
    public float PowerUsageBonus { get; init; }
}
