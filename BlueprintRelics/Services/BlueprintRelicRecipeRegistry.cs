namespace BlueprintRelics.Services;

public readonly record struct BlueprintRelicRecipePair(BlueprintRelicRecipeSpec RelicRecipe, RecipeSpec Recipe)
{
    public string Id => Recipe.Id;
}

[BindSingleton]
public class BlueprintRelicRecipeRegistry(
    FactionService factionService,
    ModdableRecipeLockSpecService recipeSpecs
) : ILoadableSingleton
{
    public static readonly ImmutableArray<BlueprintRelicRecipeRarity> AllRarities = [..
        Enum.GetValues(typeof(BlueprintRelicRecipeRarity))
        .Cast<BlueprintRelicRecipeRarity>()
        .OrderBy(q => q)];
    public static readonly ImmutableArray<BlueprintRelicSize> AllSizes = [..
        Enum.GetValues(typeof(BlueprintRelicSize))
        .Cast<BlueprintRelicSize>()
        .OrderBy(q => q)];

    public ImmutableArray<ImmutableArray<BlueprintRelicRecipePair>> RecipesByRarity { get; private set; } = [];

    public void Load()
    {
        LoadRecipes();
    }

    void LoadRecipes()
    {
        var recipes = new List<BlueprintRelicRecipePair>[AllRarities.Length];
        for (int i = 0; i < recipes.Length; i++)
        {
            recipes[i] = [];
        }

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

    public ImmutableArray<BlueprintRelicRecipePair> GetRecipesOfRarity(BlueprintRelicRecipeRarity rarity)
        => RecipesByRarity[(int)rarity];

}
