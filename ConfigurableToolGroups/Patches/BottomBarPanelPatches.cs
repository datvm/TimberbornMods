namespace ConfigurableToolGroups.Patches;

[HarmonyPatch(typeof(BottomBarPanel))]
public static class BottomBarPanelPatches
{

    [HarmonyPrefix, HarmonyPatch(nameof(BottomBarPanel.InitializeLeftSection))]
    public static bool RearrangeLeftSection(BottomBarPanel __instance) => PatchSection(__instance, RootElementLocation.Left);

    [HarmonyPrefix, HarmonyPatch(nameof(BottomBarPanel.InitializeMiddleSection))]
    public static bool RearrangeMiddleSection(BottomBarPanel __instance) => PatchSection(__instance, RootElementLocation.Middle);

    [HarmonyPrefix, HarmonyPatch(nameof(BottomBarPanel.InitializeRightSection))]
    public static bool RearrangeRightSection(BottomBarPanel __instance) => PatchSection(__instance, RootElementLocation.Right);

    static bool PatchSection(BottomBarPanel instance, RootElementLocation location)
    {
        if (ModdableCustomToolButtonService.Instance is null)
        {
            throw new InvalidOperationException("ModdableRootElementService is not initialized.");
        }

        ModdableCustomToolButtonService.Instance.InitializeSection(instance, location);
        return false;
    }

}
