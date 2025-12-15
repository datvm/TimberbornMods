namespace ModdableWeathers.Weathers.GameBuiltIns;

public class GameDroughtWeather(
    ModdableWeatherSpecService specs,
    ModdableWeatherSettingsService settingsService
) : DefaultModdableWeatherWithSettings<GameDroughtWeatherSettings>(specs, settingsService), IModdableHazardousWeather
{
    public const string WeatherId = "DroughtWeather";
    public override string Id { get; } = WeatherId;
}

public class GameDroughtWeatherSettings : DefaultModdableWeatherSettings
{
    public override void SetTo(GameModeSpec gameMode)
    {
        Enabled = true;
        StartCycle = 0;
        Chance = Mathf.RoundToInt(100 - gameMode.ChanceForBadtide * 100);
        MinDay = gameMode.DroughtDuration.Min;
        MaxDay = gameMode.DroughtDuration.Max;
        HandicapPercent = Mathf.FloorToInt(gameMode.DroughtDurationHandicapMultiplier * 100);
        HandicapCycles = gameMode.DroughtDurationHandicapCycles;
    }
}