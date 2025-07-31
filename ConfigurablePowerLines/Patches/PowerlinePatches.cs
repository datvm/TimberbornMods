using Timberborn.BlockSystem;

namespace ConfigurablePowerLines.Patches;

[HarmonyPatch]
public static class PowerlinePatches
{
    const string PowerLineTowerSpecType = "Opolame.PowerLines.Scripts.PowerLineTowerSpec";
    const string PowerLineConnectionType = "Opolame.PowerLines.Scripts.PowerLineConnectionServiceSpec";

    [HarmonyPrefix, HarmonyPatch(PowerLineTowerSpecType, "MaxConnections", MethodType.Getter)]
    public static bool PatchMaxConnections(ref int __result)
    {
        __result = MSettings.MaxConnectionsValue;
        return false;
    }

    [HarmonyPrefix, HarmonyPatch(PowerLineTowerSpecType, "MaxDistance", MethodType.Getter)]
    public static bool PatchMaxDistance(ref int __result)
    {
        __result = MSettings.MaxDistanceValue;
        return false;
    }

    [HarmonyPrefix, HarmonyPatch(PowerLineConnectionType, "MaxCableInclination", MethodType.Getter)]
    public static bool PatchMaxInclination(ref int __result)
    {
        __result = MSettings.MaxInclinationValue;
        return false;
    }

    [HarmonyPostfix, HarmonyPatch("Opolame.PowerLines.Scripts.PowerLineConnectionBlockFactory", "Load")]
    public static void PatchPowerlineBlock(object __instance)
    {
        if (!MSettings.PowerlineThroughObstaclesValue) { return; }

        BlockObjectSpec spec = (BlockObjectSpec)__instance.GetType().PropertyGetter("PowerLineConnectionBlock").Invoke(__instance, []);
        ref var blocksSpec = ref spec._blocksSpec;
        ref var blockSpecs = ref blocksSpec._blockSpecs;

        for (int i = 0; i < blockSpecs.Length; i++)
        {
            ref var blockSpec = ref blockSpecs[i];
            blockSpec._occupations = BlockOccupations.None;
        }
    }

}
