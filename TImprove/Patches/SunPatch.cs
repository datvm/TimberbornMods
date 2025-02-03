namespace TImprove.Patches;

[HarmonyPatch(typeof(Sun), "UpdateColors", [typeof(DayStageTransition)])]
public static class SunPatch
{

    public static void Prefix(ref DayStageTransition dayStageTransition)
    {
        var s = Services.ModSettings.Instance;
        if (s?.AllDayLight != true) { return; }

        var light = Enum.Parse<DayStage>(s.StaticDayLight);

        dayStageTransition = new(
            light,
            dayStageTransition.CurrentDayStageHazardousWeatherId,
            light,
            dayStageTransition.NextDayStageHazardousWeatherId,
            1);
    }

}
