namespace TImprove4UX.Patches;

[HarmonyPatch]
public static class AddHourlyProductionTimePatches
{

    [HarmonyPostfix, HarmonyPatch(typeof(ManufactoryDescriber), nameof(ManufactoryDescriber.DescribeRecipe), [typeof(RecipeSpec), typeof(float)])]
    public static void PatchStatDisplay(RecipeSpec productionRecipe, float workers, VisualElement __result)
    {
        if (!MSettings.ReverseProductionTimeValue) { return; }

        var craftingTime = productionRecipe.CycleDurationInHours / workers;

        var amounts = __result.Query<Label>(name: "Amount");
        AddPerHourValues(craftingTime, amounts);
    }

    [HarmonyPostfix, HarmonyPatch(typeof(ProductionProgressFragment), nameof(ProductionProgressFragment.ShowFragment))]
    public static void PatchProgressDisplay(ProductionProgressFragment __instance)
    {
        if (!MSettings.ReverseProductionTimeValue || !__instance._manufactory || !__instance._manufactory.HasCurrentRecipe) { return; }

        var recipe = __instance._manufactory.CurrentRecipe;
        var workers = __instance._workplace ? __instance._workplace.DesiredWorkers : 1;
        var craftingTime = recipe.CycleDurationInHours / workers;

        var amounts = __instance._root.Query<Label>(name: "Amount");
        AddPerHourValues(craftingTime, amounts);
    }

    static void AddPerHourValues(float craftingTime, UQueryBuilder<Label> labels)
    {
        foreach (var lbl in labels.ToList())
        {
            if (float.TryParse(lbl.text, out var amount))
            {
                var inOneHour = amount / craftingTime;
                lbl.text += $" ({inOneHour:0.##}/h)";
            }
            else
            {
                Debug.LogWarning($"Failed to parse amount label text: '{lbl.text}'");
            }
        }
    }

}
