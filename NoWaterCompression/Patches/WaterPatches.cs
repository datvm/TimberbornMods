global using Timberborn.WaterSystem;

namespace NoWaterCompression.Patches;

[HarmonyPatch]
public static class WaterPatches
{
    //[HarmonyPostfix, HarmonyPatch(typeof(WaterSimulator), nameof(WaterSimulator.TickSimulation))]
    //public static void AfterWaterUpdate(WaterSimulator __instance)
    //{
    //    for (int i = 0; i < __instance._waterMap._waterColumns.Length; i++)
    //    {
    //        ref var col = ref __instance._waterMap._waterColumns[i];
    //        if (col.Overflow > 0)
    //        {
    //            col.Overflow = 0;
    //        }
    //    } 
    //}

    [HarmonyPrefix, HarmonyPatch(typeof(WaterSimulatorSpec), nameof(WaterSimulatorSpec.MaxWaterfallOutflow), MethodType.Getter)]
    public static bool MaxWaterfallOutflowChange(ref float __result)
    {
        __result = float.MaxValue;
        return false;
    }

    [HarmonyPostfix, HarmonyPatch(typeof(WaterSimulator), nameof(WaterSimulator.Load))]
    public static void AfterWaterUpdate(WaterSimulator __instance)
    {
        //__instance._simulationCount = 30;
        __instance._deltaTime = .05f;
    }

}
