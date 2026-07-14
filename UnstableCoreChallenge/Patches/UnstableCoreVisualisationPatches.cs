namespace UnstableCoreChallenge.Patches;

[HarmonyPatch(typeof(UnstableCoreVisualisation))]
public static class UnstableCoreVisualisationPatches
{

    [HarmonyPrefix, HarmonyPatch(nameof(UnstableCoreVisualisation.OnSelect))]
    public static bool BlockIfStabilized(UnstableCoreVisualisation __instance)
    {
        if (!__instance) { return true; }

        var stabilizer = __instance.GetComponent<UnstableCoreStabilizer>();
        return !(stabilizer && stabilizer.IsFinished);
    }

}
