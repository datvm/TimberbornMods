namespace FiveMoreMins.Patches;

[HarmonyPatch]
public static class SleeperPatches
{

    [HarmonyPrefix, HarmonyPatch(typeof(SleeperSpec), nameof(SleeperSpec.MaxOffsetInHours), MethodType.Getter)]
    public static bool PatchMaxOffsetInHours(ref float __result)
    {
        __result = MSettings.MaxDelayValue - MSettings.MinDelayValue;
        return false;
    }

    [HarmonyPostfix, HarmonyPatch(typeof(Sleeper), nameof(Sleeper.CalculateWakeUpTimestamp))]
    public static void AddMinDelay(ref float __result)
    {
        __result += MSettings.MinDelayValue / 24f;
    }

}
