using HarmonyLib;
using Timberborn.BlockSystem;
using Timberborn.BuildingsNavigation;

namespace ExtendedBuilderReach;

[HarmonyPatch(typeof(ConstructionSiteAccessible))]
public static class ConstructionSiteAccessiblePatch
{

    [HarmonyPostfix, HarmonyPatch("MinZ", MethodType.Getter)]
    public static void MinZPostfix(ref int __result)
    {
        if (ModSettings.UnlimitedAbove)
        {
            __result = 0;
        }
        else
        {
            __result -= ModSettings.RangeAbove - 1;
        }
    }

    [HarmonyPostfix, HarmonyPatch("MaxZ", MethodType.Getter)]
    public static void MaxZPostfix(BlockObject ____blockObject, ref int __result)
    {
        if (ModSettings.UnlimitedBelow) { return; }

        __result = ____blockObject.CoordinatesAtBaseZ.z + ModSettings.RangeBelow;
    }

}
