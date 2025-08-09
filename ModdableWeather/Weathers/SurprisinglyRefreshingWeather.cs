namespace ModdableWeather.Weathers;

public class SurprisinglyRefreshingWeather(SurprisinglyRefreshingWeatherSettings settings, ModdableWeatherSpecService moddableWeatherSpecService) : DefaultModdedWeather<SurprisinglyRefreshingWeatherSettings>(settings, moddableWeatherSpecService), IModdedHazardousWeather
{
    public const string WeatherId = "SurprisinglyRefreshing";

    public override string Id { get; } = WeatherId;
}

public class SurprisinglyRefreshingWeatherSettings(ISettings settings, ModSettingsOwnerRegistry modSettingsOwnerRegistry, ModRepository modRepository, ILoc t, ModdableWeatherSpecService specs) : DefaultWeatherSettings(settings, modSettingsOwnerRegistry, modRepository, t, specs)
{
    public override string WeatherId { get; } = SurprisinglyRefreshingWeather.WeatherId;
    public override int Order { get; } = 8;
    public override string? WeatherDescLocKey { get; } = "LV.MW.SurprisinglyRefreshingDesc";

    public override WeatherParameters DefaultSettings { get; } = new(
        Chance: 10,
        MinDay: 1,
        MaxDay: 2
    );
}