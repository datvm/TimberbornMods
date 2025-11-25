namespace ConfigurableToolGroups.Patches;

[HarmonyPatch(typeof(ToolGroupService))]
public static class ToolGroupServicePatches
{

    [HarmonyPrefix, HarmonyPatch(nameof(ToolGroupService.RegisterGroup))]
    public static bool AllowMultiples(ToolGroupService __instance, ToolGroupSpec toolGroupSpec)
        => !__instance._toolGroups.ContainsKey(toolGroupSpec.Id);

}
