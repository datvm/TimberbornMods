namespace SkyblightWeather.Patches;

[HarmonyPatch]
public static class BadrainWeatherPatches
{
    [HarmonyPrefix, HarmonyPatch(typeof(ContaminationCandidatesCountingJob), nameof(ContaminationCandidatesCountingJob.GetContaminationCandidate))]
    public static bool SetContaminationIfRaining(ref float __result)
    {
        if (!BadrainWeather.IsRaining) { return true; }

        __result = 1f;
        return false;
    }
}
