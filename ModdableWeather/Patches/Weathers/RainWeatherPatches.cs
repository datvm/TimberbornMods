namespace ModdableWeather.Patches.Weathers;

[HarmonyPatch]
public static class RainWeatherPatches
{

    [HarmonyPrefix, HarmonyPatch(typeof(MoistureCalculationJob), nameof(MoistureCalculationJob.CalculateMoistureForCell))]
    public static bool SetMoistureIfRaining(ref float __result)
    {
        if (!RainWeather.IsRaining) { return true; }

        __result = 16f;
        return false;
    }

    [HarmonyPrefix, HarmonyPatch(typeof(ContaminationCandidatesCountingJob), nameof(ContaminationCandidatesCountingJob.GetContaminationCandidate))]
    public static bool SetContaminationIfRaining(ref float __result)
    {
        if (!RainWeather.IsRaining) { return true; }

        __result = 0;
        return false;
    }

}
