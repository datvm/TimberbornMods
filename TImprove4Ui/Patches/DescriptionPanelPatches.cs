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

    [HarmonyPrefix, HarmonyPatch(typeof(EntityDescriptionService), nameof(EntityDescriptionService.AddElements))]
    public static void AddScrollbarToDescription(VisualElement root)
    {
        var items = root.Q("ProductionItems");
        if (items is null) { return; }

        var ve = root.Q(classes: "grow-centered");

        var scrollBar = root
            .AddScrollView(greenDecorated: false, additionalClasses: ["game-scroll-view"])
            .SetMaxHeight(300);
        scrollBar.InsertSelfAfter(ve);
        scrollBar.Add(ve);
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
