
namespace Ziporter.Patches;

[HarmonyPatch]
public static class SunPatches
{

    [HarmonyPostfix, HarmonyPatch(typeof(DayStageCycle), nameof(DayStageCycle.GetCurrentTransition))]
    public static void OverrideDayLight(ref DayStageTransition __result)
    {
        var serv = SunLightOverrider.Instance;
        if (serv?.Override != true) { return; }

        var progress = serv.CurrentTime / serv.Parameters.Duration;

        __result = new(
            (DayStage)50,
            null,
            __result.NextDayStage,
            __result.NextDayStageHazardousWeatherId,
            progress
        );
    }

    [HarmonyPrefix, HarmonyPatch(typeof(Sun), nameof(Sun.DayStageColors))]
    public static bool OverrideDayStageColors(Sun __instance, ref DayStageColorsSpec __result, DayStage dayStage)
    {
        if ((int)dayStage == 50)
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
        if ((int)dayStage == 50)
        {
            dayStage = DayStage.Sunset;
        }
    }

}
