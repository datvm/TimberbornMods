namespace ExtendedBuilderReach.Patches;

[HarmonyPatch(typeof(ConstructionSiteAccessible))]
public static class ConstructionSiteAccessiblePatch
{

    [HarmonyPrefix, HarmonyPatch(nameof(ConstructionSiteAccessible.MinZ), MethodType.Getter)]
    public static bool ChangeMinZ(ConstructionSiteAccessible __instance, ref int __result)
    {
        __result = ModUtils.GetMinZ(__instance._blockObject.CoordinatesAtBaseZ.z);
        return false;
    }

    [HarmonyPrefix, HarmonyPatch("MaxZ", MethodType.Getter)]
    public static bool ChangeMaxZ(ConstructionSiteAccessible __instance, ref int __result)
    {
        __result = ModUtils.GetMaxZ(__instance._blockObject.CoordinatesAtBaseZ.z, __instance._mapSize.TotalSize.z);
        return false;
    }

}
