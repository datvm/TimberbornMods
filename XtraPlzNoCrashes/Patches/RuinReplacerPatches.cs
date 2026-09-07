namespace XtraPlzNoCrashes.Patches;

[HarmonyPatch]
public static class RuinReplacerPatches
{
    
    [HarmonyPrefix, HarmonyPatch(typeof(RuinReplacer), nameof(RuinReplacer.TryGetNextRuin))]
    public static bool UseHeightForRemainingYield(RuinReplacer __instance, Ruin originalRuin, out RuinSpec nextRuin, ref bool __result)
    {
        int nextHeight = HeightForYield(originalRuin);
        if (nextHeight >= originalRuin.SpecifiedHeight)
        {
            nextHeight = originalRuin.SpecifiedHeight - 1;
        }

        if (nextHeight <= 0)
        {
            nextRuin = null!;
            __result = false;
            return false;
        }

        nextRuin = __instance.GetRuinForHeight(nextHeight);
        __result = true;
        return false;
    }

    static int HeightForYield(Ruin ruin)
    {
        int yieldPerLayer = Mathf.CeilToInt((float)ruin.YielderSpec.Yield.Amount / ruin.SpecifiedHeight);
        if (yieldPerLayer <= 0)
        {
            return 0;
        }

        return Mathf.CeilToInt((float)ruin.Yielder.Yield.Amount / yieldPerLayer);
    }

}
