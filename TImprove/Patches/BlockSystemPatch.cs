namespace TImprove.Patches;

[HarmonyPatch]
public static class BlockSystemPatch
{

    [HarmonyPostfix, HarmonyPatch(typeof(BlockObjectLayoutExtensions), nameof(BlockObjectLayoutExtensions.GetPreviewCount))]
    public static void GetPreviewCount(BlockObjectLayout blockObjectLayout, ref int __result)
    {
        if (__result <= 2) { return; }

        var add = MSettings.BiggerBuildDragArea;

        if (blockObjectLayout == BlockObjectLayout.Rectangle)
        {
            var range = (int)MathF.Sqrt(__result) + add;

            __result = range * range;
        }
        else
        {
            __result += add;
        }
    }

}
