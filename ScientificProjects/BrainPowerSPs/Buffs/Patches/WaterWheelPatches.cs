﻿namespace BrainPowerSPs.Buffs.Patches;

[HarmonyPatch]
public static class WaterWheelPatches
{

    [HarmonyPostfix, HarmonyPatch(typeof(WaterPoweredGenerator), nameof(WaterPoweredGenerator.GeneratedRotation), argumentTypes: [])]
    public static void PatchGeneratorStrength(ref float __result, WaterPoweredGenerator __instance)
    {
        var buffable = __instance.GetBuffable();
        if (buffable is null) { return; }

        var effs = buffable.GetEffects<GeneratorMinStrengthBuffEff>();
        var min = effs
            .Where(q => q.Enabled)
            .Sum(q => q.Value);

        if (min > Math.Abs(__result))
        {
            __result = __result >= 0 ? min : -min;
        }
    }

}
