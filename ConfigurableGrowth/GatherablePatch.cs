using HarmonyLib;
using Timberborn.Gathering;

namespace ConfigurableGrowth;

[HarmonyPatch(typeof(Gatherable), nameof(Gatherable.YieldGrowthTimeInDays), MethodType.Getter)]
public static class GatherablePatch
{

    public static void Postfix(Gatherable __instance, ref float __result)
    {
        __result /= ModSettings.GatherableGrowthRate;
    }

}
