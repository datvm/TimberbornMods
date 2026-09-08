namespace MapTransformer.Patches;

[HarmonyPatch]
static class TransformSavePatches
{
    [HarmonyPrefix, HarmonyPatch(typeof(Ticker), nameof(Ticker.FinishFullTick))]
    static bool SkipNestedFullTick() => !Transforming.Active;
}
