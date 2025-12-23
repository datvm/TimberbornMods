namespace ModdableWeathers.Patches;

[HarmonyPatch]
public static class LandContaminationPatches
{

    [HarmonyPrefix, HarmonyPatch(typeof(ContaminationCandidatesCountingJob), nameof(ContaminationCandidatesCountingJob.GetContaminationCandidate))]
    public static bool DisableLandContamination(ref float __result)
    {
        if (!LandContaminationBlockerService.ShouldBlock) { return true; }

        __result = 0;
        return false;
    }

}
