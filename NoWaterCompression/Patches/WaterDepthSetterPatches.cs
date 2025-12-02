using System.Reflection.Emit;

namespace NoWaterCompression.Patches;

[HarmonyPatch(typeof(WaterDepthSetter))]
public static class WaterDepthSetterPatches
{

    [HarmonyPostfix, HarmonyPatch(nameof(WaterDepthSetter.SetWaterDepth))]
    public static void SetOverflowTo0(ref WaterColumn waterColumn)
    {
        waterColumn.Overflow = 0;
    }

}
