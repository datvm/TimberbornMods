namespace BrainPowerSPs.Patches;

[HarmonyPatch(typeof(WaterPoweredGenerator))]
public static class WaterPoweredGeneratorPatches
{

    [HarmonyPostfix, HarmonyPatch(nameof(WaterPoweredGenerator.GeneratedRotation), [])]
    public static void SetMinimumRotation(WaterPoweredGenerator __instance, ref float __result)
    {
        var comp = __instance.GetComponentFast<WaterWheelPowerSPComponent>();
        if (!comp || Mathf.Abs(__result) >= comp.MinimumGeneratorStrength) { return; }

        __result = __result < 0 ? -comp.MinimumGeneratorStrength : comp.MinimumGeneratorStrength;
    }

}
