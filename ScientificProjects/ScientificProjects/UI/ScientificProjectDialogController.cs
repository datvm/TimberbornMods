namespace ScientificProjects.UI;

public class ScientificProjectDialogController(
    BasicStatisticsPanel statPanel,
    EventBus eb,
    BindableButtonFactory bindableButtonFac,
    ISettings settings,
    IContainer container,
    DialogService dialogService,
    ScientificProjectDailyService dailyService,
    IAssetLoader assets,
    DevModeManager devModeManager,
    InputService inputService
) : ILoadableSingleton
{
    static readonly string FirstIntroKey = $"ScientificProjects.FirstIntro";

    const string Keybinding = "ScientificProjectDialog";
    
    public Texture2D DefaultIcon { get; private set; } = null!;

    public void Load()
    {
        DefaultIcon = assets.Load<Texture2D>("Sprites/TopBar/Science");

        RegisterScienceClick();
        eb.Register(this);

        ShowFirstTimeIntro();
    }

    void RegisterScienceClick()
    {
        var btnScienceHeader = statPanel._root.Q<VisualElement>(name: "ScienceCountHeader")
            ?? throw new InvalidOperationException("ScienceCountHeader not found");

        bindableButtonFac.CreateAndBind(btnScienceHeader, Keybinding, ShowScienceDialog);
    }

    async void ShowFirstTimeIntro()
    {
        if (settings.GetBool(FirstIntroKey)) { return; }

        settings.SetBool(FirstIntroKey, true);

        dialogService.Alert("LV.SP.Welcome", true);
        await ShowScienceDialogAsync();
    }

    public async void ShowScienceDialog() => await ShowScienceDialogAsync();
    public async Task<bool> ShowScienceDialogAsync() => await ShowScienceDialogAsync(null);
    public async Task<bool> ShowScienceDialogAsync(Action<ScientificProjectDialog>? configure)
    {
        var diag = container.GetInstance<ScientificProjectDialog>();
        diag.Init();
        configure?.Invoke(diag);

        return await diag.ShowAsync();
    }

    [OnEvent]
    public async void OnNotEnoughDailySciencePayment(OnScientificProjectDailyNotEnoughEvent _)
    {
        var result = await ShowScienceDialogAsync(diag => diag.AddNotEnoughPanel());

        if (result) // Pay
        {
            dailyService.PayForToday();
        }
        else // Skip
        {
            dailyService.SkipTodayPayment();
        }
    }

    public bool ShouldAllowDevUnlock => devModeManager.Enabled && inputService.IsKeyHeld(BuildingToolLocker.InstantUnlockKey);

}
