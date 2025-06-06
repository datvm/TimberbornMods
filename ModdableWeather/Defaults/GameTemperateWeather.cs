
namespace ModdableWeather.Defaults;

public class GameTemperateWeather(GameTemperateWeatherSettings settings, ModdableWeatherSpecService moddableWeatherSpecService) : DefaultModdedWeather<GameTemperateWeatherSettings>(settings, moddableWeatherSpecService), IModdedTemperateWeather
{
    public const string WeatherId = "TemperateWeather";

    public override string Id { get; } = WeatherId;
}

public class GameTemperateWeatherSettings(ISettings settings, ModSettingsOwnerRegistry modSettingsOwnerRegistry, ModRepository modRepository, ILoc t, ModdableWeatherSpecService specs, ModSettingsBox modSettingsBox) : DefaultWeatherDifficultySettings(settings, modSettingsOwnerRegistry, modRepository, t, specs, modSettingsBox)
{
    public override string WeatherId { get; } = GameTemperateWeather.WeatherId;
    public override string ModId { get; } = nameof(ModdableWeather);
    public override int Order { get; } = 1;

    protected override WeatherParameters GetDifficultyParameters(WeatherDifficulty difficulty) => StaticGetDifficultyParameters(difficulty);

    static WeatherParameters StaticGetDifficultyParameters(WeatherDifficulty difficulty)
    {
        var v = ModdableWeatherUtils.GetGameSettingsAtDifficulty(difficulty);

        return new(
            Enabled: true,
            StartCycle: 0,
            Chance: 100,
            MinDay: v.TemperateWeatherDuration.Min,
            MaxDay: v.TemperateWeatherDuration.Max
        );
    }
}