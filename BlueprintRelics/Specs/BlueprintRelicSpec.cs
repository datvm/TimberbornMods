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
    public ImmutableArray<GoodAmountSpec> PossibleRequiredGoods { get; init; }

    [Serialize]
    public int RequiredGoodsCount { get; init; }

    [Serialize]
    public int MaxScienceRequirement { get; init; }

}
#nullable enable

public enum BlueprintRelicSize
{
    Small,
    Medium,
    Large
}
