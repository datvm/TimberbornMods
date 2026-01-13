namespace BlueprintRelics.Specs;

#nullable disable
public record BlueprintRelicRaritySpec : ComponentSpec
{

    [Serialize]
    public string SizeId { get; init; }

    [Serialize]
    public ImmutableArray<int> RarityChance { get; init; } = [];

}
#nullable enable