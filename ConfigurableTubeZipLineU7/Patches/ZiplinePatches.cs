namespace ConfigurableTubeZipLine.Patches;

[HarmonyPatch]
public static class ZiplinePatches
{

    [HarmonyPrefix, HarmonyPatch(typeof(ZiplineCableNavMeshSpec), nameof(ZiplineCableNavMeshSpec.CableUnitCost), MethodType.Getter)]
    public static bool ChangeZiplineSpeed(ref float __result)
    {
        __result = MSettings.CalculateCost(MSettings.ZiplineSpeed);
        return false;
    }

    [HarmonyPrefix, HarmonyPatch(typeof(ZiplineConnectionServiceSpec), nameof(ZiplineConnectionServiceSpec.MaxCableInclination), MethodType.Getter)]
    public static bool ChangeZiplineMaxInclination(ref int __result)
    {
        __result = MSettings.ZiplineMaxInclination;
        return false;
    }

    [HarmonyPrefix, HarmonyPatch(typeof(ZiplineTowerSpec), nameof(ZiplineTowerSpec.MaxConnections), MethodType.Getter)]
    public static bool ChangeZiplineMaxConnections(ref int __result)
    {
        __result = MSettings.ZiplineMaxConnection;
        return false;
    }

    [HarmonyPrefix, HarmonyPatch(typeof(ZiplineTowerSpec), nameof(ZiplineTowerSpec.MaxDistance), MethodType.Getter)]
    public static bool ChangeZiplineMaxDistance(ref int __result)
    {
        __result = MSettings.ZiplineMaxDistance;
        return false;
    }

    [HarmonyPostfix, HarmonyPatch(typeof(ZiplineConnectionBlockFactory), nameof(ZiplineConnectionBlockFactory.Load))]
    public static void MakeZiplineCableNonsolid(ZiplineConnectionBlockFactory __instance)
    {
        if (!MSettings.ZiplineThroughObstacles) { return; }

        ref var blocksSpec = ref __instance.ZiplineConnectionBlock._blocksSpec;
        ref var blockSpecs = ref blocksSpec._blockSpecs;

        for (int i = 0; i < blockSpecs.Length; i++)
        {
            ref var blockSpec = ref blockSpecs[i];
            blockSpec._occupations = BlockOccupations.None;
        }
    }

}
