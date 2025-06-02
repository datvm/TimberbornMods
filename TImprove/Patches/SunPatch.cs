namespace TImprove.Patches;

[HarmonyPatch(typeof(Sun))]
public static class SunPatch
{

    [HarmonyPrefix, HarmonyPatch(nameof(Sun.UpdateColors))]
    public static void SetStaticDayLight(ref DayStageTransition dayStageTransition)
    {
        var s = MSettings.Instance;
        if (s?.AllDayLight != true) { return; }

        var light = Enum.Parse<DayStage>(s.StaticDayLight);

        dayStageTransition = new(
            light,
            dayStageTransition.CurrentDayStageHazardousWeatherId,
            light,
            dayStageTransition.NextDayStageHazardousWeatherId,
            1);
    }

    [HarmonyPrefix, HarmonyPatch(nameof(Sun.UpdateRotation))]
    public static bool AvoidUpdateRotation()
    {
        return MSettings.Instance?.DisableShadowRotation != true;
    }

}
