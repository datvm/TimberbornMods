namespace ModdableWeathers.UI.Settings;

public class WeatherSettingsDialogShower(
    IOptionsBox optionsBox,
    IContainer container,
    ILoc t,
    WeatherHistoryRegistry history,
    WeatherGenerator generator,
    GameCycleService gameCycleService,
    GeneralGlobalSettings generalGlobalSettings,
    EventBus eb,
    ISingletonLoader loader
) : ILoadableSingleton, ISaveableSingleton
{
    static readonly SingletonKey SaveKey = new(nameof(WeatherSettingsDialogShower));
    static readonly PropertyKey<bool> DialogShowedKey = new("DialogShowed");

    bool dialogShowed;

    readonly GameOptionsBox optionsBox = (GameOptionsBox)optionsBox;

    public void Load()
    {
        var settingsButton = optionsBox._root.Q("SettingsButton");

        var weatherSettingsButton = settingsButton.AddMenuButton(t.T("LV.MW.ShowSettings"),
            onClick: ShowDialog,
            name: "WeatherSettings",
            stretched: true);
        weatherSettingsButton.InsertSelfBefore(settingsButton);

        dialogShowed = loader.TryGetSingleton(SaveKey, out var s) && s.Has(DialogShowedKey) && s.Get(DialogShowedKey);

        eb.Register(this);
    }

    [OnEvent]
    public void OnShowPrimaryUI(ShowPrimaryUIEvent _)
    {
        if (generalGlobalSettings.DoNotShowOnNewGame || dialogShowed) { return; }

        ShowDialog();
    }

    public void Save(ISingletonSaver singletonSaver)
    {
        var s = singletonSaver.GetSingleton(SaveKey);
        s.Set(DialogShowedKey, true);
    }

    public async void ShowDialog()
    {
        await ShowDialogAsync();
    }

    public async Task ShowDialogAsync()
    {
        var diag = container.GetInstance<WeatherSettingsDialog>();
        await diag.ShowAsync();

        var cycle = gameCycleService.Cycle;
        history.ClearFutureEntries(cycle);
        generator.EnsureWeatherGenerated(cycle);
    }

}
