namespace BlueprintRelics.Specs;

#nullable disable
public record BlueprintRelicSpec : ComponentSpec
{

    [Serialize]
    public BlueprintRelicSize Size { get; init; }

    [Serialize]
    public MinMaxSpec<int> ExpireInDays { get; init; }

    [Serialize]
    public int ExcavationSteps { get; init; }

    [Serialize]
    public float ExcavationStepDays { get; init; }

    [Serialize]
    public ImmutableArray<BlueprintRelicRequiredGoodsSpec> RequiredGoodGroups { get; init; }

    [Serialize]
    public int MaxScienceRequirement { get; init; }

    [Serialize]
    public ImmutableArray<int> RecipeRarityChance { get; init; }

    [Serialize]
    public float NegotiateCooldownDays { get; init; }

}

public record BlueprintRelicRequiredGoodsSpec : ComponentSpec
{
    [Serialize]
    public int Index { get; init; }

    [Serialize]
    public ImmutableArray<string> GoodIds { get; init; }

    [Serialize]
    public int MaxAmount { get; init; }
}

#nullable enable

public enum BlueprintRelicSize
{
    Small,
    Medium,
    Large
}
