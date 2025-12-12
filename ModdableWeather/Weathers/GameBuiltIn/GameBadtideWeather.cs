namespace ModdableWeather.Weathers.GameBuiltIn;

public class GameBadtideWeather(WeatherEntitySettingEntry weatherSettingsService)
    : ModdableWeatherWithSettings<GameBadtideWeatherSettings>(weatherSettingsService), IModdableHazardousWeather
{
    public const string WeatherId = "BadtideWeather";

    public override string Id { get; } = WeatherId;

}

public class GameBadtideWeatherSettings : WeatherSettingEntry
{
    public override string EntityId { get; } = GameTemperateWeather.WeatherId;

    public override void SetValueForDifficulty(GameModeSpec gameMode, bool firstTime)
    {
        Enabled = true;
        StartCycle = 0;
        Chance = Mathf.RoundToInt(gameMode.ChanceForBadtide * 100);
        MinDay = gameMode.BadtideDuration.Min;
        MaxDay = gameMode.BadtideDuration.Max;
        HandicapPerc = Mathf.FloorToInt(gameMode.BadtideDurationHandicapMultiplier * 100f);
        HandicapCycles = gameMode.BadtideDurationHandicapCycles;
    }

}
