#if TIMBER7

using Timberborn.BlueprintSystem;
using Timberborn.Goods;
using Timberborn.WaterBuildings;
using Timberborn.Workshops;

namespace ConfigurablePumps.Patches;

[HarmonyPatch]
public static class SpecPatches
{
    const string WaterId = "Water";
    const string BadwaterId = "Badwater";

    public static float? OriginalMechPumpWater { get; private set; }

    static readonly PropertyInfo GoodAmountSetter = typeof(GoodAmountSpecNew).Property(nameof(GoodAmountSpecNew.Amount));
    [HarmonyPostfix, HarmonyPatch(typeof(BasicDeserializer), nameof(BasicDeserializer.Deserialize))]
    public static void PatchDeserialize(ref object __result)
    {
        if (__result is WaterMoverSpec wms)
        {
            OriginalMechPumpWater = wms._waterPerSecond;
            wms._waterPerSecond = MSettings.MechPumpWater;
        }
        else if (MSettings.WaterProdMultiplier != 1
            && __result is RecipeSpec r
            && (r.Ingredients.Length == 0 || r.Products.Length == 0)
        )
        {
            foreach (var item in r.Ingredients)
            {
                if (item.Id == WaterId || item.Id == BadwaterId)
                {
                    GoodAmountSetter.SetValue(item, (int)MathF.Ceiling(item.Amount * MSettings.WaterProdMultiplier));
                }
            }

            foreach (var item in r.Products)
            {
                if (item.Id == WaterId || item.Id == BadwaterId)
                {
                    GoodAmountSetter.SetValue(item, (int)MathF.Ceiling(item.Amount * MSettings.WaterProdMultiplier));
                }
            }
        }
    }

}

#endif