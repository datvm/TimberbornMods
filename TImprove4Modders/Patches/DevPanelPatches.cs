namespace TImprove4Modders.Patches;

[HarmonyPatch]
public static class DevPanelPatches
{

    [HarmonyPrefix, HarmonyPatch(typeof(DevPanel), nameof(DevPanel.ResetFilter))]
    public static bool PatchResetFilter() => !MSettings.NoClearDevFilter;

    [HarmonyPostfix, HarmonyPatch(typeof(DevPanel), nameof(DevPanel.ShowAllButtons))]
    public static void AfterShowAllButtons(DevPanel __instance) => ModFilter(__instance);

    [HarmonyPostfix, HarmonyPatch(typeof(DevPanel), nameof(DevPanel.SaveFavouritesAndRebuildPanel))]
    public static void AfterSaveFavouritesAndRebuildPanel(DevPanel __instance) => ModFilter(__instance);

    static void ModFilter(DevPanel panel)
    {
        var text = panel._filter?.text;
        if (!MSettings.NoClearDevFilter || string.IsNullOrEmpty(text)) { return; }

        panel.FilterButtons(text);
    }
}
