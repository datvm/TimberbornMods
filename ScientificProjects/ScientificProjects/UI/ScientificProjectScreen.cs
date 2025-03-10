namespace ScientificProjects.UI;

public class ScientificProjectScreen(
    VisualElementInitializer veInit,
    PanelStack panelStack,
    ILoc t,
    BasicStatisticsPanel statPanel,
    ScientificProjectService projects,
    IAssetLoader assets,
    DialogBoxShower diagShower,
    InputService input,
    DevModeManager devMode,
    EventBus eb,
    ScienceService sciences,
    BindableButtonFactory bindableButtonFac
) : ILoadableSingleton, IUnloadableSingleton
{
    const string Keybinding = "ScientificProjectDialog";

    VisualElement btnScienceHeader = null!;

    public void Load()
    {
        RegisterScienceClick();

        eb.Register(this);
    }

    void RegisterScienceClick()
    {
        btnScienceHeader = statPanel._root.Q<VisualElement>(name: "ScienceCountHeader")
            ?? throw new InvalidOperationException("ScienceCountHeader not found");

        bindableButtonFac.CreateAndBind(btnScienceHeader, Keybinding, OnScienceHeaderClick);
    }

    public void ShowScienceDialog(OnScientificProjectDailyNotEnoughEvent? notEnough = default)
    {
        var diag = new ScientificProjectDialog(t, projects, assets, diagShower, input, sciences);

        if (notEnough is not null)
        {
            diag.AddNotEnoughScience(notEnough.Value);
        }

        if (devMode.Enabled)
        {
            diag.AddDevMode();
        }

        diag.Show(veInit, panelStack);
    }

    public void Unload()
    {
        eb.Unregister(this);
    }

    void OnScienceHeaderClick()
    {
        ShowScienceDialog();
    }

    [OnEvent]
    public void OnScienceNotEnough(OnScientificProjectDailyNotEnoughEvent ev)
    {
        ShowScienceDialog(notEnough: ev);
    }

}
