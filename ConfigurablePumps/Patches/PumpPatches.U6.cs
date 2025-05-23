﻿#if TIMBER6
using Timberborn.WaterBuildings;

namespace ConfigurablePumps;

[HarmonyPatch]
public static class PumpPatches
{

    [HarmonyPostfix, HarmonyPatch(typeof(WaterInputSpecification), nameof(WaterInputSpecification.MaxDepth), MethodType.Getter)]
    public static void PatchMaxDepth(ref int __result)
    {
        if (MSettings.AllMultiplier)
        {
            __result = (int)MathF.Ceiling(__result * MSettings.Multiplier);
        }
        else if (MSettings.AllFixedDepth)
        {
            __result = MSettings.FixedDepth;
        }
    }

}

#endif