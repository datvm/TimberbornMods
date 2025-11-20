namespace TImprove.Patches;

[HarmonyPatch(typeof(DayStageCycle))]
public static class DayStageCyclePatches
{
    const int TotalDayStages = (int)DayStage.Night + 1;

    [HarmonyPrefix, HarmonyPatch(nameof(DayStageCycle.GetCurrentTransition))]
    public static bool SetStaticDayLight(DayStageCycle __instance, ref DayStageTransition __result)
    {
        var value = MSettings.Instance?.AllDayStage;
        if (value is null) { return true; }

        var curr = value.Value;
        var next = (DayStage)(((int)curr + 1) % TotalDayStages);

        __result = __instance.Transition(curr, next, 24f);

        return false;
    }

}
