namespace ConfigurableToolGroups.Patches;

[HarmonyPatch(typeof(ToolButtonService))]
public static class ToolButtonServicePatches
{

    [HarmonyPrefix, HarmonyPatch(nameof(ToolButtonService.Add), [typeof(ToolButton)])]
    public static void TrackButton(ToolButton toolButton) => BottomBarButtonLookupService.Instance?.RegisterToolButton(toolButton);


    [HarmonyPrefix, HarmonyPatch(nameof(ToolButtonService.Add), [typeof(ToolGroupButton)])]
    public static void TrackButton(ToolGroupButton toolButton) => BottomBarButtonLookupService.Instance?.RegisterToolGroupButton(toolButton);

}
