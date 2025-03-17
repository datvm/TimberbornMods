namespace UnlockableRecipe.RecipeManagers;

[HarmonyPatch]
public static class RecipeModPatch
{

    [HarmonyPrefix, HarmonyPatch(typeof(RecipeSpec), nameof(RecipeSpec.BackwardCompatibleIds), MethodType.Setter)]
    public static void FixBackwardCompatibleIds(RecipeSpec __instance, ref ImmutableArray<string> value)
    {
        // Currently the game code does not handle this properly and causes crash
        // Because it assigns default, which can't be iterated of used
        // See https://github.com/dotnet/sdk/issues/47647
        if (value == default)
        {
            value = [];
        }
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
            Debug.Log($"{nameof(UnlockableRecipe)}: Complete list of recipes for {prefab.Name}: {string.Join(", ", list)}");
        }
    }

}
