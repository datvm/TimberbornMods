global using Timberborn.ZiplineSystem;


namespace ConfigurableTubeZipLine.Patches;

[HarmonyPatch]
public static class ZiplinePatches
{

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

        ref var blocksSpec = ref __instance.ZiplineConnectionBlock._blocksSpecification;

        for (int i = 0; i < blocksSpec._blockSpecifications.Length; i++)
        {
            ref var blockSpec = ref blocksSpec._blockSpecifications[i];
            blockSpec._occupations = Timberborn.BlockSystem.BlockOccupations.None;
        }
    }

}
