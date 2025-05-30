namespace ModdableWeather.Defaults;

public class GameDroughtWeather(GameDroughtWeatherSettings settings, ModdableWeatherSpecService moddableWeatherSpecService) : DefaultModdedWeather<GameDroughtWeatherSettings>(settings, moddableWeatherSpecService), IModdedHazardousWeather
{
    public const string WeatherId = "DroughtWeather";

    public override string Id { get; } = WeatherId;
}

public class GameDroughtWeatherSettings(
    ISettings settings,
    ModSettingsOwnerRegistry modSettingsOwnerRegistry,
    ModRepository modRepository,
    ILoc t,
    ModdableWeatherSpecService specs
) : DefaultWeatherSettings(settings, modSettingsOwnerRegistry, modRepository, t, specs)
{
    public override string WeatherId { get; } = GameDroughtWeather.WeatherId;
    public override string ModId { get; } = nameof(ModdableWeather);

    public override WeatherParameters DefaultSettings { get; } = new(
        true,
        0,
        60,
        5, 9,
        38, 5);
}
