using Timberborn.BlueprintSystem;
using Timberborn.WaterBuildings;
using Timberborn.Workshops;

namespace ConfigurablePumps.Patches;

[HarmonyPatch]
public static class SpecPatches
{
    const string WaterId = "Water";
    const string BadwaterId = "Badwater";
    static readonly ImmutableHashSet<string> WaterIds = [WaterId, BadwaterId];

    public static float? OriginalMechPumpWater { get; private set; }

    static readonly MethodInfo CycleDurationInHoursSetter = typeof(RecipeSpec).PropertySetter(nameof(RecipeSpec.CycleDurationInHours));
    [HarmonyPostfix, HarmonyPatch(typeof(BasicDeserializer), nameof(BasicDeserializer.Deserialize))]
    public static void PatchDeserialize(ref object __result)
    {
        if (__result is WaterMoverSpec wms)
        {
            OriginalMechPumpWater = wms._waterPerSecond;
            wms._waterPerSecond = MSettings.MechPumpWater;
        }
        else if (MSettings.WaterProdTimeMultiplier != 1
            && __result is RecipeSpec r
            && (r.Ingredients.Length == 0 || r.Products.Length == 0)
            && (r.Ingredients.Any(q => WaterIds.Contains( q.Id ))  || r.Products.Any(q => WaterIds.Contains(q.Id)))
        )
        {
            CycleDurationInHoursSetter.Invoke(r, [r.CycleDurationInHours * MSettings.WaterProdTimeMultiplier]);
        }
    }

}
