namespace TImprove4Ui.Patches;

[HarmonyPatch]
public static class DescriptionPanelPatches
{

    static WeakReference<VisualElement>? toolPanel;

    [HarmonyPrefix, HarmonyPatch(typeof(ToolPanel), nameof(ToolPanel.AddFragments))] 
    public static void SetupToolPanel(VisualElement root)
    {
        root.style.minWidth = root.style.maxWidth = root.style.width = new Length(100, LengthUnit.Percent);

        toolPanel = new(root);
        ChangeToolPanelPosition(null);
    }

    public static void ChangeToolPanelPosition(string? pos)
    {
        if (toolPanel is null || !toolPanel.TryGetTarget(out var el)) { return; }

        el.style.alignItems = (pos ?? MSettings.ToolDescPos) switch
        {
            "Left" => Align.FlexStart,
            "Right" => Align.FlexEnd,
            _ => Align.Center,
        };
    }

}
