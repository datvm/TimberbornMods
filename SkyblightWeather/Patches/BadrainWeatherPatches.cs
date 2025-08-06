namespace SkyblightWeather.Patches;

[HarmonyPatch]
public static class BadrainWeatherPatches
{
    [HarmonyPostfix, HarmonyPatch(typeof(MoistureCalculationJob), nameof(MoistureCalculationJob.CalculateMoistureForCell))]
    public static void LimitIfRaining(ref float __result)
    {
        var limit = BadrainWeather.ShouldReduceMoisture;
        if (limit is null || __result <= limit) { return; }

        __result = limit.Value;
    }

    [HarmonyPrefix, HarmonyPatch(typeof(ContaminationCandidatesCountingJob), nameof(ContaminationCandidatesCountingJob.GetContaminationCandidate))]
    public static bool ContaminateLand(ref float __result)
    {
        if (!BadrainWeather.ShouldContaminateLand) { return true; }

        __result = 1f;
        return false;
    }

}
