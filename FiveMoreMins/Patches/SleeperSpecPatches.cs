namespace FiveMoreMins.Patches;

[HarmonyPatch(typeof(SleeperSpec))]
public static class SleeperSpecPatches
{

    [HarmonyPrefix, HarmonyPatch(nameof(SleeperSpec.MaxOffsetInHours), MethodType.Getter)]
    public static bool PatchMaxOffsetInHours(ref float __result)
    {
        __result = MSettings.MaxDelayValue;
        return false;
    }

}
