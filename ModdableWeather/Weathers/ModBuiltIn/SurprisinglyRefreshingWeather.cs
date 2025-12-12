using ModdableWeather.Services.Registries;

namespace ModdableWeather.Weathers.ModBuiltIn;

public class SurprisinglyRefreshingWeather(SurprisinglyRefreshingWeatherSettings settings, ModdableWeatherSpecRegistry moddableWeatherSpecService) : DefaultModdedWeather<SurprisinglyRefreshingWeatherSettings>(settings, moddableWeatherSpecService), IModdableHazardousWeather
{
    public const string WeatherId = "SurprisinglyRefreshing";
    public override string Id { get; } = WeatherId;
}

public class SurprisinglyRefreshingWeatherSettings : WeatherSettingEntry
{
    public override string EntityId { get; } = SurprisinglyRefreshingWeather.WeatherId;

    public override bool CanSupport(GameModeSpec gameMode) => gameMode.IsDefault;
    public override void SetValueForDifficulty(GameModeSpec gameMode, bool firstTime)
    {
        Chance = 10;
        MinDay = 1;
        MaxDay = 2;
    }
}