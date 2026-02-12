namespace ConfigurableToolGroups.Patches;

[HarmonyPatch(typeof(TutorialStageService))]
public static class TutorialStageServicePatches
{

    [HarmonyPrefix, HarmonyPatch(nameof(TutorialStageService.GetSteps))]
    public static bool Bypass(ref IEnumerable<TutorialStep> __result)
    {
        if (!TutorialSettingsService.TutorialsDisabled)
        {
            return true;
        }   

        __result = [];
        return false;
    }

}
