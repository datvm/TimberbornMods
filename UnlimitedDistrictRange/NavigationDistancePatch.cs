using HarmonyLib;
using Timberborn.Navigation;

namespace UnlimitedDistrictRange;

[HarmonyPatch(typeof(NavigationDistance), nameof(NavigationDistance.DistrictTerrain), MethodType.Getter)]
public static class NavigationDistancePatch
{

    public static bool Prefix(ref int __result)
    {
        __result = 500;
        return false;
    }

}
