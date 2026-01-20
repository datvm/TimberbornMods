namespace BlueprintRelics.Specs;

public record BlueprintRelicRecipeSpec : ComponentSpec
{

    [Serialize]
    public BlueprintRelicRecipeRarity Rarity { get; init; }

    [Serialize]
    public string? BuildingNameLoc { get; init; }

    [Serialize(nameof(BuildingNameLoc))]
    public LocalizedText? BuildingName { get; init; }

    [Serialize]
    public ImmutableArray<string> Factions { get; init; } = [];

}

public enum BlueprintRelicRecipeRarity
{
    Common,
    Uncommon,
    Rare
}