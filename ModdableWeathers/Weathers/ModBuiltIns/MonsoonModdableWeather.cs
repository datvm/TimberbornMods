
namespace ModdableWeathers.Weathers.ModBuiltIns;

public class MonsoonModdableWeather(ModdableWeatherSpecService specs, ModdableWeatherSettingsService settingsService)
    : DefaultModdableWeatherWithSettings<MoonsoonWeatherSettings>(specs, settingsService), IModdableHazardousWeather
{
    public const string WeatherId = "Monsoon";
    public override string Id { get; } = WeatherId;
}

public class MoonsoonWeatherSettings : DefaultModdableWeatherSettings
{

    public override void SetTo(GameModeSpec gameMode)
    {
        Enabled = true;
        StartCycle = Mathf.CeilToInt(gameMode.CyclesBeforeRandomizingBadtide / 2f);
        Chance = Mathf.FloorToInt(gameMode.ChanceForBadtide * 100f);
        MinDay = gameMode.DroughtDuration.Min;
        MaxDay = gameMode.DroughtDuration.Max;
        HandicapPercent = Mathf.FloorToInt(gameMode.DroughtDurationHandicapMultiplier * 100f);
        HandicapCycles = gameMode.DroughtDurationHandicapCycles;
    }

}
