global using Timberborn.AssetSystem;
global using Timberborn.WellbeingUI;
global using Timberborn.Debugging;

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
    DevModeManager devMode
) : ILoadableSingleton
{
    VisualElement btnScienceHeader = null!;

    public void Load()
    {
        btnScienceHeader = statPanel._root.Q<VisualElement>(name: "ScienceCountHeader")
            ?? throw new InvalidOperationException("ScienceCountHeader not found");

        btnScienceHeader.RegisterCallback<ClickEvent>(OnScienceHeaderClick);
    }

    public void ShowScienceDialog()
    {
        var diag = new ScientificProjectDialog(t, projects, assets, diagShower, input);

        if (devMode.Enabled)
        {
            diag.AddDevMode();
        }

        diag.Show(veInit, panelStack);
    }

    void OnScienceHeaderClick(ClickEvent _)
    {
        ShowScienceDialog();
    }

}
