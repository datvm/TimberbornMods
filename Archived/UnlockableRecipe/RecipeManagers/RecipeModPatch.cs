namespace UnlockableRecipe.RecipeManagers;

[HarmonyPatch]
public static class RecipeModPatch
{

    [HarmonyPostfix, HarmonyPatch(typeof(RecipeSpec), nameof(RecipeSpec.BackwardCompatibleIds), MethodType.Getter)]
    public static void FixBackwardCompatibleIds(ref ImmutableArray<string> __result)
    {
        // Currently the game code does not handle this properly and causes crash
        // Because it assigns default, which can't be iterated of used
        // See https://github.com/dotnet/sdk/issues/47647
        if (__result == default) { __result = []; }
    }

    [HarmonyPostfix, HarmonyPatch(typeof(PrefabGroupService), nameof(PrefabGroupService.Load))]
    public static void AddRecipes(PrefabGroupService __instance)
    {
        var addingRecipes = RecipeModRegistry.AddedRecipes;
        var removingRecipes = RecipeModRegistry.RemovedRecipes;
        if ((addingRecipes is null || addingRecipes.Count == 0)
            && (removingRecipes is null || removingRecipes.Count == 0))
        {
            return;
        }

        var recipes = __instance._specService.GetSpecs<RecipeSpec>()
            .ToFrozenDictionary(q => q.Id);

        foreach (var p in __instance.AllPrefabs)
        {
            var man = p.GetComponent<ManufactorySpec>();
            if (man is null) { continue; }

            var prefab = p.GetComponent<PrefabSpec>();
            if (prefab is null) { continue; }

            var adding = addingRecipes?.TryGetValue(prefab.Name, out var currAddings) == true;
            var removing = removingRecipes?.TryGetValue(prefab.Name, out var currRemovings) == true;
            if (!adding && !removing) { continue; }

            IEnumerable<string> list = man._productionRecipeIds;
            if (adding)
            {
                Debug.Log($"{nameof(UnlockableRecipe)}: Adding recipes to {prefab.Name}: {string.Join(", ", currAddings)}");
                list = list
                    .Concat(currAddings)
                    .Distinct();
            }

            if (removing)
            {
                Debug.Log($"{nameof(UnlockableRecipe)}: Removing recipes from {prefab.Name}: {string.Join(", ", currRemovings)}");
                list = list
                    .Except(currRemovings);
            }

            man._productionRecipeIds = [.. list];
            ValidateRecipes(prefab.Name, list, recipes);

            Debug.Log($"{nameof(UnlockableRecipe)}: Complete list of recipes for {prefab.Name}: {string.Join(", ", list)}");
        }
    }

    static void ValidateRecipes(string prefabName, IEnumerable<string> recipeIds, FrozenDictionary<string, RecipeSpec> recipes)
    {
        HashSet<string> output = [];

        foreach (var id in recipeIds)
        {
            var recipe = recipes[id];

            foreach (var p in recipe.Products)
            {
                output.Add(p.Id);
            }
        }

        foreach (var id in recipeIds)
        {
            var recipe = recipes[id];

            foreach (var i in recipe.Ingredients)
            {
                if (output.Contains(i.Id))
                {
                    throw new InvalidDataException($"Recipe {id} for {prefabName} contains an ingredient that is also a product: {i.Id}. This will cause issue with Hauler");
                }
            }
        }
    }

}
