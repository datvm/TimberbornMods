namespace ConfigurableBeaverWalk.Patches;

[HarmonyPatch(typeof(GoodCarrier))]
public static class GoodCarrierPatch
{

    [HarmonyPostfix, HarmonyPatch(nameof(GoodCarrier.LiftingCapacity), MethodType.Getter)]
    public static void ChangeLiftingCapacity(GoodCarrier __instance, ref int __result)
    {
        var usingValue = MSettings.DifferentForBots ?
            (__instance.HasComponent<BotSpec>() ? MSettings.BotCarryingWeightMultiplier : MSettings.CarryingWeightMultiplier)
            : MSettings.CarryingWeightMultiplier;
        if (usingValue == 1) { return; }

        __result = (int)(__result * usingValue);
    }

}
