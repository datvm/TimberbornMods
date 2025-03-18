namespace UnlockableRecipe;

public class RecipeModRegistry(IEnumerable<RecipeAdder> adders, IEnumerable<RecipeRemover> removers, ISpecService specs) : ILoadableSingleton
{
    public static FrozenDictionary<string, ImmutableArray<string>>? AddedRecipes { get; private set; }
    public static FrozenDictionary<string, ImmutableArray<string>>? RemovedRecipes { get; private set; }

    public void Load()
    {
        var addings = Map(adders);
        var removing = Map(removers);

        ApplySpecs();

        AddedRecipes = addings.ToFrozenDictionary(q => q.Key, q => q.Value.Distinct().ToImmutableArray());
        RemovedRecipes = removing.ToFrozenDictionary(q => q.Key, q => q.Value.Distinct().ToImmutableArray());

        void ApplySpecs()
        {
            var modSpecs = specs.GetSpecs<RecipeModderSpec>();
            if (modSpecs?.Any() != true) { return; }

            foreach (var spec in modSpecs)
            {
                var dict = spec.Remove ? removing : addings;
                foreach (var prefabName in spec.PrefabNames)
                {
                    if (!dict.TryGetValue(prefabName, out var recipes))
                    {
                        dict[prefabName] = recipes = [];
                    }

                    recipes.AddRange(spec.RecipeIds);
                }
            }
        }
    }

    static Dictionary<string, List<string>> Map<T>(IEnumerable<T> modifiers)
        where T : RecipeModification
    {
        return modifiers
            .SelectMany(q => q.Recipes)
            .GroupBy(q => q.Key)
            .ToDictionary(q => q.Key, q => q.Select(r => r.Value).ToList());
    }

}
