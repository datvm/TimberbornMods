namespace TechTree.UI;

[BindSingleton]
public class TechTreeTriggerUI(
    BasicStatisticsPanel panel,
    ILoc t,
    TechTreeDialog diag,
    VisualElementLoader veLoader,
    ITooltipRegistrar tooltipRegistrar,
    IAssetLoader assets,
    EventBus eb,
    BindableToggleFactory bindableToggleFactory,
    UILayout uiLayout
) : ILoadableSingleton
{
    public const string HotkeyId = "OpenTechTree";

#nullable disable
    VisualElement btnOpen;
    Toggle chkOpen;
#nullable enable

    public void Load()
    {
        AddDedicatedButton();
        AddSciencePanelTrigger();

        eb.Register(this);
    }

    void AddDedicatedButton()
    {
        btnOpen = veLoader.LoadVisualElement("Common/SquareToggle");
        tooltipRegistrar.RegisterWithKeyBinding(btnOpen, t.T("LV.TT.Open"), HotkeyId);

        chkOpen = btnOpen.Q<Toggle>("Toggle");
        var checkMark = btnOpen.Q(className: "unity-toggle__checkmark");
        var icon = assets.Load<Texture2D>("UI/Images/Game/science-icon");
        checkMark.style.backgroundImage = icon;
    }

    void AddSciencePanelTrigger()
    {
        var pnlScience = panel._root.Q("ScienceCountHeader");
        pnlScience.RegisterCallback<ClickEvent>(_ => OpenTechTree());
    }

    [OnEvent]
    public void OnShowPrimaryUI(ShowPrimaryUIEvent _)
    {
        bindableToggleFactory.CreateAndBind(chkOpen, HotkeyId, OnToggle, () => false);
        uiLayout.AddTopRightButton(btnOpen, 4);
    }

    void OnToggle(bool _)
    {
        chkOpen.SetValueWithoutNotify(false);
        OpenTechTree();
    }

    void OpenTechTree() => _ = OpenTechTreeAsync();

    async Task OpenTechTreeAsync()
    {
        await diag.ShowAsync();
        chkOpen.SetValueWithoutNotify(false);
    }

}
