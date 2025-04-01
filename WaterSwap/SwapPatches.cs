global using Timberborn.WaterSourceSystem;

namespace WaterSwap;

[HarmonyPatch]
public static class SwapPatches
{

    [HarmonyPostfix, HarmonyPatch(typeof(WaterSource), nameof(WaterSource.Contamination), MethodType.Getter)]
    public static void SwapWaterContamination(ref float __result)
    {
        __result = Mathf.Clamp(1 - __result, 0f, 1f);
    }

}
