﻿namespace ScientificProjects.UI;

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
    BindableButtonFactory bindableButtonFac,
    ISettings settings
) : ILoadableSingleton, IUnloadableSingleton
{
    static readonly string FirstIntroKey = $"ScientificProjects.FirstIntro";

    bool firstIntroShowed;
    const string Keybinding = "ScientificProjectDialog";

    VisualElement btnScienceHeader = null!;

    public void Load()
    {
        LoadSavedData();

        RegisterScienceClick();
        eb.Register(this);

        if (!firstIntroShowed)
        {
            firstIntroShowed = true;
            settings.SetBool(FirstIntroKey, true);

            ShowScienceDialog(firstIntro: true);
        }
    }

    void LoadSavedData()
    {
        firstIntroShowed = settings.GetBool(FirstIntroKey);
    }

    void RegisterScienceClick()
    {
        btnScienceHeader = statPanel._root.Q<VisualElement>(name: "ScienceCountHeader")
            ?? throw new InvalidOperationException("ScienceCountHeader not found");

        bindableButtonFac.CreateAndBind(btnScienceHeader, Keybinding, OnScienceHeaderClick);
    }

    public void ShowScienceDialog(OnScientificProjectDailyNotEnoughEvent? notEnough = default, bool firstIntro = false)
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

        if (firstIntro)
        {
            diagShower.Create()
                .SetMessage("LV.SP.Welcome".T(t))
                .Show();
        }
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
