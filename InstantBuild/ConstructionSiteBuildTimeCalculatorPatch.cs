using HarmonyLib;
using Timberborn.ConstructionSites;

namespace InstantBuild;

[HarmonyPatch(typeof(ConstructionSiteBuildTimeCalculator), nameof(ConstructionSiteBuildTimeCalculator.GetConstructionTimeInHours))]
public static class ConstructionSiteBuildTimeCalculatorPatch
{

    public static bool Prefix(ref float __result)
    {
        __result = .01f;
        return false;
    }

}
