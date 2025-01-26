using HarmonyLib;
using Timberborn.Carrying;

namespace ConfigurableBeaverWalk;

[HarmonyPatch(typeof(GoodCarrier), nameof(GoodCarrier.LiftingCapacity), MethodType.Getter)]
public static class GoodCarrierPatch
{
    
    public static void Postfix(ref int __result)
    {
        if (ModSettings.CarryingWeightMultiplier == 1) { return; }

        __result = (int)(__result * ModSettings.CarryingWeightMultiplier);
    }

}
