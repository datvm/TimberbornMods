namespace ModdableWeathers.UI.Settings;

public class WeatherSettingsDialogShower(
    IOptionsBox optionsBox,
    IContainer container,
    ILoc t
) : ILoadableSingleton
{

    readonly GameOptionsBox optionsBox = (GameOptionsBox)optionsBox;

    public void Load()
    {
        var settingsButton = optionsBox._root.Q("SettingsButton");

        var weatherSettingsButton = settingsButton.AddMenuButton(t.T("LV.MW.ShowSettings"),
            onClick: ShowDialog,
            name: "WeatherSettings",
            stretched: true);
        weatherSettingsButton.InsertSelfBefore(settingsButton);
    }

    public async void ShowDialog()
    {
        await ShowDialogAsync();
    }

    public async Task ShowDialogAsync()
    {
        var diag = container.GetInstance<WeatherSettingsDialog>();
        await diag.ShowAsync();
    }

}
