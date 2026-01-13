namespace BlueprintRelics.Specs;

public record BlueprintRelicRecipeUpgradeSpec : ComponentSpec
{
    [Serialize]
    public float TimeReduction { get; init; }

    [Serialize]
    public float CapacityIncrease { get; init; }

    [Serialize]
    public int AdditionalOutput { get; init; }

    [Serialize]
    public int MaximumChoices { get; init; }

    [Serialize]
    public float ScienceRewardChance { get; init; }

    [Serialize]
    public float CapacityRewardChance { get; init; }

    [Serialize]
    public float TimeReductionRewardChance { get; init; }

    [Serialize]
    public float AdditionalOutputRewardChance { get; init; }
}
