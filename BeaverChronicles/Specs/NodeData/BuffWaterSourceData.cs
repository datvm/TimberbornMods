namespace BeaverChronicles.Specs.NodeData;

public record BuffWaterSourceData
{
    public ImmutableArray<string>? EntityIds { get; init; }
    public BuffInfo BuffInfo { get; init; } = new();
    public BuffWaterSourceEffects Effects { get; init; } = new();
}

public record BuffWaterSourceEffects
{
    public string? ImmuneToDrought { get; init; }
    public string? ImmuneToBadtide { get; init; }
    public string? StrengthMultiplier { get; init; }
    public string? ContaminationDelta { get; init; }
}
