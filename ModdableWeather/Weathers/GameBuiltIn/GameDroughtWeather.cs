namespace ModdableWeather.Weathers.GameBuiltIn;

public class GameDroughtWeather(WeatherEntitySettingEntry weatherSettingsService) 
    : ModdableWeatherWithSettings<GameDroughtWeatherSettings>(weatherSettingsService), IModdableHazardousWeather
{
    public const string WeatherId = "DroughtWeather";

    public override string Id { get; } = WeatherId;

}


public class GameDroughtWeatherSettings : WeatherSettingEntry
{
    public override string EntityId { get; } = GameTemperateWeather.WeatherId;

    public override void SetValueForDifficulty(GameModeSpec gameMode, bool firstTime)
    {
        Enabled = true;
        StartCycle = 0;
        Chance = Mathf.FloorToInt(100 - gameMode.ChanceForBadtide * 100f);
        MinDay = gameMode.DroughtDuration.Min;
        MaxDay = gameMode.DroughtDuration.Max;
        HandicapPerc = Mathf.FloorToInt(gameMode.DroughtDurationHandicapMultiplier * 100f);
        HandicapCycles = gameMode.DroughtDurationHandicapCycles;
    }
}