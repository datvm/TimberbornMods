namespace ScienceShop.Patches;

[HarmonyPatch]
static class ManufactoryDescriberPatches
{
    [HarmonyPostfix, HarmonyPatch(typeof(ManufactoryDescriber), nameof(ManufactoryDescriber.DescribeRecipe), [typeof(RecipeSpec)])]
    public static void AddToProgress((VisualElement input, VisualElement output) __result, RecipeSpec productionRecipe)
        => ConsumeScienceService.Instance?.AddToRecipeVisuals(productionRecipe, __result.input);

    [HarmonyPostfix, HarmonyPatch(typeof(ManufactoryDescriber), nameof(ManufactoryDescriber.DescribeRecipe), [typeof(RecipeSpec), typeof(float)])]
    public static void AddToPreview(VisualElement __result, RecipeSpec productionRecipe)
        => ConsumeScienceService.Instance?.AddToRecipeVisuals(productionRecipe, __result);
}
