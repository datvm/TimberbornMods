namespace BlueprintRelics.Services;

public readonly record struct BlueprintRelicRecipePair(BlueprintRelicRecipeSpec RelicRecipe, RecipeSpec Recipe)
{
    public string Id => Recipe.Id;
}

public class BlueprintRelicRecipeRegistry(
    ISpecService specs,
    FactionService factionService,
    ModdableRecipeLockSpecService recipeSpecs
) : ILoadableSingleton
{
    public static readonly ImmutableArray<BlueprintRelicRecipeRarity> AllRarities = [..
        Enum.GetValues(typeof(BlueprintRelicRecipeRarity))
        .Cast<BlueprintRelicRecipeRarity>()
        .OrderBy(q => q)];

    ImmutableArray<BlueprintRelicRaritySpec> raritySpecsBySize = [];
    public ImmutableArray<ImmutableArray<BlueprintRelicRecipePair>> RecipesByRarity { get; private set; } = [];

    public void Load()
    {
        LoadRaritiesSpecs();
        LoadRecipes();
    }

    void LoadRaritiesSpecs()
    {
        var rarities = new BlueprintRelicRaritySpec[AllRarities.Length];
        foreach (var s in specs.GetSpecs<BlueprintRelicRaritySpec>())
        {
            rarities[(int)Enum.Parse<BlueprintRelicRecipeRarity>(s.SizeId)] = s;
        }

        for (int i = 0; i < rarities.Length; i++)
        {
            if (rarities[i] is null)
            {
                throw new InvalidOperationException($"No {nameof(BlueprintRelicRaritySpec)} registered for size {AllRarities[i]}.");
            }
        }

        raritySpecsBySize = [.. rarities];
    }

    void LoadRecipes()
    {
        var recipes = new List<BlueprintRelicRecipePair>[AllRarities.Length];

        var faction = factionService.Current.Id;
        foreach (var recipe in recipeSpecs.ModdableRecipes)
        {
            var spec = recipe.ModdableRecipe.GetSpec<BlueprintRelicRecipeSpec>();
            if (spec is null
                || (spec.Factions.Length > 0 && !spec.Factions.Contains(faction))) { continue; }

            recipes[(int)spec.Rarity].Add(new(spec, recipe.Recipe));
        }

        RecipesByRarity = [.. recipes.Select(q => q.ToImmutableArray())];
    }

    public BlueprintRelicRaritySpec GetRarity(BlueprintRelicSize size) => raritySpecsBySize[(int)size];

    public ImmutableArray<BlueprintRelicRecipePair> GetRecipesOfRarity(BlueprintRelicRecipeRarity rarity)
        => RecipesByRarity[(int)rarity];

}
