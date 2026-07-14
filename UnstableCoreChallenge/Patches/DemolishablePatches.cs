namespace UnstableCoreChallenge.Patches;

[HarmonyPatch(typeof(Demolishable))]
public static class DemolishablePatches
{

    [HarmonyPrefix, HarmonyPatch(nameof(Demolishable.Mark))]
    public static bool CancelMarkIfNotStabilize(Demolishable __instance)
    {
        var stabilizer = __instance.GetComponent<UnstableCoreStabilizer>();
        return !stabilizer || stabilizer.IsFinished;
    }

    [HarmonyPrefix, HarmonyPatch(nameof(Demolishable.ShowDemolishButtonInEntityPanel), MethodType.Getter)]
    public static bool InterceptForUnstableCore(Demolishable __instance, ref bool __result)
    {
        if (!__instance) { return false; }

        var stabilizer = __instance.GetComponent<UnstableCoreStabilizer>();
        if (stabilizer && !stabilizer.IsFinished)
        {
            __result = false;
            return false;
        }

        return true;
    }

}
