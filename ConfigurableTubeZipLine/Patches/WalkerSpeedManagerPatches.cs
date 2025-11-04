namespace ConfigurableTubeZipLine.Patches;

[HarmonyPatch(typeof(WalkerSpeedManager))]
public static class WalkerSpeedManagerPatches
{

    [HarmonyPrefix, HarmonyPatch(nameof(WalkerSpeedManager.GetWalkerSpeedAtCurrentPosition))]
    public static bool RemoveWaterPenalty(WalkerSpeedManager __instance, ref float __result)
    {
        if (!MSettings.NoWaterPenalty) { return true; }

        __result = __instance.GetWalkerBaseSpeed();
        return false;
    }

}
