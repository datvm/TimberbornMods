namespace CraneHeads.Components;

public record CraneHeadWindmillSpec : ComponentSpec
{
    [Serialize]
    public float BonusPerSection { get; init; }

    [Serialize]
    public float MinRequiredWindStrength { get; init; }
}
