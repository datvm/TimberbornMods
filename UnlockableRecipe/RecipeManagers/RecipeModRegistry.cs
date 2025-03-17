namespace UnlockableRecipe;

public class RecipeModRegistry(IEnumerable<RecipeAdder> adders, IEnumerable<RecipeRemover> removers) : ILoadableSingleton
{
    public static FrozenDictionary<string, ImmutableArray<string>>? AddedRecipes { get; private set; }
    public static FrozenDictionary<string, ImmutableArray<string>>? RemovedRecipes { get; private set; }

    public void Load()
    {
        AddedRecipes = Map(adders);
        RemovedRecipes = Map(removers);
    }

    static FrozenDictionary<string, ImmutableArray<string>> Map<T>(IEnumerable<T> modifiers)
        where T : RecipeModification
    {
        return modifiers
            .SelectMany(q => q.Recipes)
            .GroupBy(q => q.Key)
            .ToFrozenDictionary(q => q.Key, q => q.Select(r => r.Value).ToImmutableArray());
    }

}
