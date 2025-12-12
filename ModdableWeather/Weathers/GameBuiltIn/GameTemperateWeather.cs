namespace ModdableWeather.Weathers.GameBuiltIn;

public class GameTemperateWeather(WeatherEntitySettingEntry weatherSettingsService)
    : ModdableWeatherWithSettings<GameTemperateWeatherSettings>(weatherSettingsService)
    , IModdableBenignWeather
{
    public const string WeatherId = "TemperateWeather";

    public override string Id { get; } = WeatherId;
}

public class GameTemperateWeatherSettings : WeatherSettingEntry
{
    public override string EntityId { get; } = GameTemperateWeather.WeatherId;

    public override void SetValueForDifficulty(GameModeSpec gameMode, bool firstTime)
    {
        Enabled = true;
        StartCycle = 0;
        Chance = 100;
        MinDay = gameMode.TemperateWeatherDuration.Min;
        MaxDay = gameMode.TemperateWeatherDuration.Max;
    }
}
