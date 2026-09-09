namespace MapTransformer.Patches;

[HarmonyPatch]
static class HidableModelsPatches
{
    [HarmonyPrefix, HarmonyPatch(typeof(HidableModels), nameof(HidableModels.ModelsAt))]
    static void GrowForLevel(HidableModels __instance, int level)
        => Grow(__instance, level);

    [HarmonyPrefix, HarmonyPatch(typeof(HidableModels), nameof(HidableModels.Add))]
    static void GrowForAdd(HidableModels __instance, BlockObjectModelController model)
        => GrowForModel(__instance, model);

    [HarmonyPrefix, HarmonyPatch(typeof(HidableModels), nameof(HidableModels.Remove))]
    static void GrowForRemove(HidableModels __instance, BlockObjectModelController model)
        => GrowForModel(__instance, model);

    static void GrowForModel(HidableModels hidableModels, BlockObjectModelController model)
    {
        var (_, top) = HidableModels.GetModelRange(model);
        Grow(hidableModels, Math.Max(top, hidableModels._mapSize.TotalSize.z));
    }

    static void Grow(HidableModels hidableModels, int maxIndexInclusive)
    {
        if (maxIndexInclusive < 0)
        {
            return;
        }

        var models = hidableModels._models;
        while (models.Count <= maxIndexInclusive)
        {
            models.Add(new HashSet<BlockObjectModelController>());
        }
    }
}
