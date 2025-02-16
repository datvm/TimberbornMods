#if TIMBER6
using Timberborn.Goods;

namespace ConfigurablePumps.Patches;

[HarmonyPatch]
public static class SpecPatches
{
    const string WaterId = "Water";
    const string BadwaterId = "Badwater";
    static readonly ImmutableHashSet<string> WaterIds = new[] { WaterId, BadwaterId }.ToImmutableHashSet();

    [HarmonyPostfix, HarmonyPatch(typeof(RecipeSpecification), nameof(RecipeSpecification.CycleDurationInHours), MethodType.Getter)]
    public static void PatchCycleDurationInHours(RecipeSpecification __instance, ref float __result)
    {
        if (MSettings.WaterProdTimeMultiplier != 1
            && (__instance.Ingredients.Count == 0 || __instance.Products.Count == 0)
            && (__instance.Ingredients.Any(q => WaterIds.Contains(q.GoodId)) || __instance.Products.Any(q => WaterIds.Contains(q.GoodId))))
        {
            __result *= MSettings.WaterProdTimeMultiplier;
        }
    }

}
#endif