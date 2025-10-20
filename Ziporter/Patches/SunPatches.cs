namespace Ziporter.Patches;

[HarmonyPatch]
public static class SunPatches
{

    [HarmonyPrefix, HarmonyPatch(typeof(Sun), nameof(Sun.DayStageColors))]
    public static bool OverrideDayStageColors(Sun __instance, ref DayStageColorsSpec __result)
    {
        if (SunLightOverrider.Instance?.Override == true)
        {
            __result = SunLightOverrider.Instance?.DayStageColorsSpec ?? 
                __instance.DayStageColors(DayStage.Day);
            return false;
        }
        else
        {
            return true;
        }
    }

    [HarmonyPrefix, HarmonyPatch(typeof(SkyboxPositioner), nameof(SkyboxPositioner.DayProgress))]
    public static void OverrideDayProgress(ref DayStage dayStage)
    {
        if (SunLightOverrider.Instance?.Override == true)
        {
            dayStage = DayStage.Sunset;
        }
    }

}
