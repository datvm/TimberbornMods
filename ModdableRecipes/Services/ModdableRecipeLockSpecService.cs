namespace ModdableRecipes.Services;

public readonly record struct ModdableRecipeLockSpecPair(RecipeSpec Recipe, ModdableRecipeLockSpec ModdableRecipe)
{
    public string Id => Recipe.Id;
}

public class ModdableRecipeLockSpecService(RecipeSpecService specs, FactionService factions) : ILoadableSingleton
{

    public ImmutableArray<ModdableRecipeLockSpecPair> ModdableRecipes { get; private set; } = [];
    public FrozenDictionary<string, ModdableRecipeLockSpecPair> ModdableRecipeById { get; private set; } = FrozenDictionary<string, ModdableRecipeLockSpecPair>.Empty;

    public void Load()
    {
        List<ModdableRecipeLockSpecPair> recipes = [];
        foreach (var recipe in specs.GetRecipes())
        {
            var moddable = recipe.GetSpec<ModdableRecipeLockSpec>();

            if (moddable is not null)
            {
                recipes.Add(new ModdableRecipeLockSpecPair(recipe, moddable));
            }
        }

        ModdableRecipes = [.. recipes];
        ModdableRecipeById = ModdableRecipes.ToFrozenDictionary(x => x.Id);
    }

    public ModdableRecipeLockTitle GetTitleVisibility(string id)
    {
        var moddable = ModdableRecipeById[id].ModdableRecipe;

        return moddable.VisibleFactions.IsEmpty || moddable.VisibleFactions.Contains(factions.Current.Id)
            ? moddable.LockTitle
            : ModdableRecipeLockTitle.Hidden;
    }

}
