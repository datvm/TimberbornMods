namespace TImprove4Ui.Patches;

[HarmonyPatch]
public static class WorkplacesBatchControlTabPatches
{

    [HarmonyPrefix, HarmonyPatch(typeof(BatchControlTab), nameof(WorkplacesBatchControlTab.GetHeader))]
    public static bool OnGetHeader(BatchControlTab __instance, ref VisualElement? __result)
    {
        if (__instance is not WorkplacesBatchControlTab) { return true; }

        __result = WorkplacesBatchControlTabService.Instance?.CreateHeader();
        return false;
    }

    [HarmonyPostfix, HarmonyPatch(typeof(BatchControlTab), nameof(WorkplacesBatchControlTab.UpdateRowsVisibility))]
    public static void UpdateRowsVisibility(BatchControlTab __instance)
    {
        if (__instance is not WorkplacesBatchControlTab) { return; }

        WorkplacesBatchControlTabService.Instance?.ApplyFilter();
    }

}
