namespace SkyblightWeather.Patches;

[HarmonyPatch]
public static class BadrainEffectPatches
{

    [HarmonyPostfix, HarmonyPatch(typeof(MoistureCalculationTask), nameof(MoistureCalculationTask.CalculateMoistureForCell))]
    public static void LimitMoistureRange(ref float __result)
    {
        var limit = LandEffectService.LimitMoistureRange;
        if (limit is null || __result <= limit) { return; }

        __result = limit.Value;
    }

    [HarmonyPrefix, HarmonyPatch(typeof(ContaminationCandidatesCountingTask), nameof(ContaminationCandidatesCountingTask.GetContaminationCandidate))]
    public static bool ContaminateLand(ref float __result)
    {
        if (!LandEffectService.ShouldContaminateLand) { return true; }

        __result = 1f;
        return false;
    }

}
