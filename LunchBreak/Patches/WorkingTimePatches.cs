using Timberborn.WorkSystem;

namespace LunchBreak.Patches;

[HarmonyPatch]
public static class WorkingTimePatches
{

    [HarmonyPostfix, HarmonyPatch(typeof(WorkingHoursManager), nameof(WorkingHoursManager.AreWorkingHours), MethodType.Getter)]
    public static void AddLunchBreak(ref bool __result)
    {
        if (!__result || LunchBreakManager.Instance is null) { return; }

        if (LunchBreakManager.Instance.IsLunchBreakTime)
        {
            __result = false;
        }
    }

}
