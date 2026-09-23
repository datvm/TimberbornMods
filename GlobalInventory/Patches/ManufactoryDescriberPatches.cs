namespace GlobalInventory.Patches;

[HarmonyPatch(typeof(ManufactoryDescriber))]
static class ManufactoryDescriberPatches
{
    [HarmonyPostfix, HarmonyPatch(nameof(ManufactoryDescriber.DescribeRecipe), [typeof(RecipeSpec)])]
    public static void AddToProgress((VisualElement input, VisualElement output) __result, RecipeSpec productionRecipe)
        => GlobalInventoryManufactoryService.Instance?.AddToRecipeVisuals(productionRecipe, __result.input, __result.output);

    [HarmonyPostfix, HarmonyPatch(nameof(ManufactoryDescriber.DescribeRecipe), [typeof(RecipeSpec), typeof(float)])]
    public static void AddToPreview(VisualElement __result, RecipeSpec productionRecipe)
        => GlobalInventoryManufactoryService.Instance?.AddToRecipeVisuals(productionRecipe, __result);
}
