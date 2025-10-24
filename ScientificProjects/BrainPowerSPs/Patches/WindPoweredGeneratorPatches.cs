namespace BrainPowerSPs.Patches;

[HarmonyPatch(typeof(WindPoweredGenerator))]
public static class WindPoweredGeneratorPatches
{

    [HarmonyPostfix, HarmonyPatch(nameof(WindPoweredGenerator.GeneratorStrength), MethodType.Getter)]
    public static void MultiplyPowerFromheight(WindPoweredGenerator __instance, ref float __result)
    {
        var comp = __instance.GetComponentFast<WindPowerSPComponent>();
        if (!comp) { return; }

        __result *= comp.HeightMultiplier;
    }

}
