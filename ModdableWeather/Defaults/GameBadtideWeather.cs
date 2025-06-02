namespace ModdableWeather.Defaults;

public class GameBadtideWeather(
    GameBadtideWeatherSettings settings,
    ModdableWeatherSpecService moddableWeatherSpecService
) : DefaultModdedWeather<GameBadtideWeatherSettings>(settings, moddableWeatherSpecService), IModdedHazardousWeather
{
    public const string WeatherId = "BadtideWeather";

    public override string Id { get; } = WeatherId;

}

public class GameBadtideWeatherSettings(
    ISettings settings,
    ModSettingsOwnerRegistry modSettingsOwnerRegistry,
    ModRepository modRepository,
    ILoc t,
    ModdableWeatherSpecService specs
) : DefaultWeatherSettings(settings, modSettingsOwnerRegistry, modRepository, t, specs)
{
    public override string WeatherId { get; } = GameBadtideWeather.WeatherId;
    public override string ModId { get; } = nameof(ModdableWeather);

    public override WeatherParameters DefaultSettings { get; } = new(
        true,
        4,
        40,
        4, 8,
        15, 5);
}
