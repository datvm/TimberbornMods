namespace ConfigurableToolGroups.Patches;

[HarmonyPatch(typeof(DescriptionPanelController))]
public static class DescriptionPanelPatches
{
    const int ButtonHeight = 54;

    [HarmonyPostfix, HarmonyPatch(nameof(DescriptionPanelController.Show))]
    public static void PositionAfterShow(DescriptionPanel panel)
    {
        var el = panel.Root;
        el.style.position = Position.Relative;
        el.style.top = -ButtonHeight * (ButtonsPatches.CurrentLevel + 1);
    }

}
