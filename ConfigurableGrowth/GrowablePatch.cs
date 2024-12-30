using HarmonyLib;
using Timberborn.Cutting;
using Timberborn.Gathering;
using Timberborn.Growing;

namespace ConfigurableGrowth;

[HarmonyPatch(typeof(Growable), nameof(Growable.GrowthTimeInDays), MethodType.Getter)]
public static class GrowablePatch
{

    public static void Postfix(Growable __instance, ref float __result)
    {
        float mul;
        if (__instance.GetComponentInParent<Cuttable>() is not null)
        {
            mul = ModSettings.TreeGrowthRate;
        }
        else if (__instance.GetComponentInParent<Gatherable>() is not null)
        {
            mul = ModSettings.CropGrowthRate;
        }
        else
        {
            mul = ModSettings.OtherGrowthRate;
        }

        __result /= mul;
    }

}
