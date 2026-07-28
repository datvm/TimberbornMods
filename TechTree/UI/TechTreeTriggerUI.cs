namespace TechTree.UI;

[BindSingleton]
public class TechTreeTriggerUI(
    ILoc t,
    TechTreeDialog diag,
    ITooltipRegistrar tooltipRegistrar,
    EventBus eb,
    BindableButtonFactory bindableButtonFactory,
    VisualElementInitializer veInit,
    PopulationPanel populationPanel
) : ILoadableSingleton
{
    public const string HotkeyId = "OpenTechTree";

    public void Load()
    {
        eb.Register(this);
    }

    [OnEvent]
    public void OnShowPrimaryUI(ShowPrimaryUIEvent _)
    {
        var container = populationPanel._root.Q("Counters");

        var btnOpen = container.AddChild<NineSliceButton>(classes: ["population-button", "square-large--green"])
            .SetHeight(60f)
            .JustifyContent().AlignItems()
            .SetMarginBottom(5);

        btnOpen.AddGameLabel(
            "PLACEHOLDER",
            size: UiBuilder.GameLabelSize.Big,
            color: UiBuilder.GameLabelColor.Yellow,
            bold: true,
            centered: true);

        btnOpen.Initialize(veInit);

        tooltipRegistrar.RegisterWithKeyBinding(btnOpen, t.T("LV.TT.Open"), HotkeyId);
        bindableButtonFactory.CreateAndBind(btnOpen, HotkeyId, OpenTechTree);
    }

    void OpenTechTree() => _ = OpenTechTreeAsync();

    async Task OpenTechTreeAsync()
    {
        await diag.ShowAsync();
    }

}
