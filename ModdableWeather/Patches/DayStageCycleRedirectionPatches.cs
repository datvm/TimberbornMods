namespace ModdableWeather.Patches;

[HarmonyPatch(typeof(DayStageCycle))]
public static class DayStageCycleRedirectionPatches
{

    [HarmonyPrefix, HarmonyPatch(nameof(DayStageCycle.Transition), [typeof(DayStage), typeof(DayStage), typeof(float)])]
    public static bool TransitionPrefix(DayStage currentDayStage, DayStage nextDayStage, float hoursToNextDayStage, ref DayStageTransition __result)
    {
        __result = ModdableDayStageCycle.Instance.NewTransition(currentDayStage, nextDayStage, hoursToNextDayStage);
        return false;
    }

}
