#if TIMBER6
using Timberborn.Goods;

namespace ConfigurablePumps.Patches;

[HarmonyPatch]
public static class SpecPatches
{
    const string WaterId = "Water";
    const string BadwaterId = "Badwater";

    [HarmonyPostfix, HarmonyPatch(typeof(GoodAmount), nameof(GoodAmount.Amount), MethodType.Getter)]
    public static void PatchWaterAmount(ref int __result, GoodAmount __instance)
    {
        if (MSettings.WaterProdMultiplier == 1) { return; }

        if (__instance.GoodId == WaterId || __instance.GoodId == BadwaterId)
        {
            __result = (int)MathF.Ceiling(__result * MSettings.WaterProdMultiplier);
        }
    }

}
#endif