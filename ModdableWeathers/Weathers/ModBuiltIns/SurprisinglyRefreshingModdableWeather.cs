namespace ModdableWeathers.Weathers.ModBuiltIns;

public class SurprisinglyRefreshingModdableWeather(ModdableWeatherSpecService specs, ModdableWeatherSettingsService settingsService) 
    : DefaultModdableWeatherWithSettings<SurprisinglyRefreshingWeatherSettings>(specs, settingsService), IModdableHazardousWeather
{
    public const string WeatherId = "SurprisinglyRefreshing";
    public override string Id { get; } = WeatherId;
}

public class SurprisinglyRefreshingWeatherSettings : DefaultModdableWeatherSettings
{

    public override bool CanSupport(GameModeSpec gameMode) => gameMode.IsDefault;
    public override void SetTo(GameModeSpec gameMode)
    {
        Enabled = true;
        Chance = 10;
        MinDay = 1;
        MaxDay = 2;
    }
}