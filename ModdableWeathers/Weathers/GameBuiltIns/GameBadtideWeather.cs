namespace ModdableWeathers.Weathers.GameBuiltIns;

public class GameBadtideWeather(
    ModdableWeatherSpecService specs,
    ModdableWeatherSettingsService settingsService
) : DefaultModdableWeatherWithSettings<GameBadtideWeatherSettings>(specs, settingsService), IModdableHazardousWeather
{
    public const string WeatherId = "BadtideWeather";
    public override string Id { get; } = WeatherId;
}

public class GameBadtideWeatherSettings : DefaultModdableWeatherSettings
{
    public override void SetTo(GameModeSpec gameMode)
    {
        Enabled = true;
        StartCycle = gameMode.CyclesBeforeRandomizingBadtide;
        Chance = Mathf.RoundToInt(gameMode.ChanceForBadtide * 100);
        MinDay = gameMode.BadtideDuration.Min;
        MaxDay = gameMode.BadtideDuration.Max;
        HandicapPercent = Mathf.FloorToInt(gameMode.BadtideDurationHandicapMultiplier * 100);
        HandicapCycles = gameMode.BadtideDurationHandicapCycles;
    }
}