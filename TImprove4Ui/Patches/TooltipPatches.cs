namespace TImprove4Ui.Patches;

[HarmonyPatch]
public static class TooltipPatches
{

    [HarmonyPostfix, HarmonyPatch(typeof(Tooltip), nameof(Tooltip.UpdateTooltipContent))]
    public static void AddPickingMode(Tooltip __instance)
    {
        SetPickingMode(__instance._tooltipLabel);
    }

    static void SetPickingMode(VisualElement el)
    {
        el.pickingMode = PickingMode.Ignore;
        foreach (var c in el.Children())
        {
            SetPickingMode(c);
        }
    }

}
