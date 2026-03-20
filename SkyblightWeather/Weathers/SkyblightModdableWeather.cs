namespace SkyblightWeather.Weathers;

public class SkyblightModdableWeather(ModdableWeatherSpecService specs, ModdableWeatherSettingsService settingsService) : DefaultModdableWeatherWithSettings<SkyblightWeatherSettings>(specs, settingsService), IModdableHazardousWeather
{
    public const string WeatherId = "Skyblight";

    public override string Id { get; } = WeatherId;

}

public class SkyblightWeatherSettings : DefaultModdableWeatherSettings
{
    public override void InitializeNewValuesTo(GameModeSpec gameMode)
    {
        Enabled = true;
        StartCycle = gameMode.CyclesBeforeRandomizingBadtide;
        Chance = Mathf.FloorToInt(gameMode.ChanceForBadtide * 100f);
        MinDay = gameMode.BadtideDuration.Min;
        MaxDay = gameMode.BadtideDuration.Max;
        HandicapPercent = Mathf.FloorToInt(gameMode.BadtideDurationHandicapMultiplier * 100f);
        HandicapCycles = gameMode.BadtideDurationHandicapCycles;
    }
}