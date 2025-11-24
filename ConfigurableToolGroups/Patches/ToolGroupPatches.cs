namespace ConfigurableToolGroups.Patches;

[HarmonyPatch]
public static class ToolGroupPatches
{

    [HarmonyPrefix, HarmonyPatch(typeof(ToolGroupService), nameof(ToolGroupService.EnterToolGroup))]
    public static void OnEnterToolGroup(ToolGroupService __instance, ToolGroupSpec toolGroup)
    {
        TimberUiUtils.LogDev($"Prev: {__instance.ActiveToolGroup?.Id}, Coming: {toolGroup?.Id}");
    }

}
