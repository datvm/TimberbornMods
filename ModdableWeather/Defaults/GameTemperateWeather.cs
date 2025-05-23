namespace ModdableWeather.Defaults;

public class GameTemperateWeather(GameTemperateWeatherSettings settings) : DefaultModdedWeather<GameTemperateWeatherSettings>(settings), IModdedTemperateWeather
{
    public const string WeatherId = "TemperateWeather";

    public override string Id { get; } = WeatherId;
}

public class GameTemperateWeatherSettings(ISettings settings, ModSettingsOwnerRegistry modSettingsOwnerRegistry, ModRepository modRepository, ILoc t, ModdableWeatherSpecService specs) : DefaultWeatherSettings(settings, modSettingsOwnerRegistry, modRepository, t, specs)
{
    public override string WeatherId { get; } = GameTemperateWeather.WeatherId;
    public override string ModId { get; } = nameof(ModdableWeather);

    public override WeatherParameters DefaultSettings { get; } = new(
        true,
        0,
        100,
        13, 17,
        100, 0);

}