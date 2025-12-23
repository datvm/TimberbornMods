namespace ModdableWeathers.Weathers.Settings;

public abstract class DefaultModdableWeatherWithSettings<TSetting>(
    ModdableWeatherSpecService specs,
    ModdableWeatherSettingsService settingsService
) : ModdableWeatherBase(specs), IModdableWeatherWithSettings<TSetting> where TSetting : IDefaultModdableWeatherSettings, new()
{

    public override bool Enabled => Settings.Enabled;

    public TSetting Settings { get; protected set; } = default!;

    public override void Load()
    {
        base.Load();
        Settings = settingsService.GetSettings<TSetting>();
    }

    public override int GetChance(WeatherCycleStageDecision stageDecision, WeatherCycleDecision cycleDecision, WeatherHistoryService history)
        => cycleDecision.Cycle < Settings.StartCycle ? 0 : Settings.Chance;

    public override int GetDuration(WeatherCycleStageDecision stageDecision, WeatherCycleDecision cycleDecision, WeatherHistoryService history)
    {
        var s = Settings;
        var handicap = ModdableWeathersUtils.CalculateHandicap(
            () => history.GetWeatherOccurrenceCount(Id),
            s.HandicapCycles,
            () => s.HandicapPercent
        );

        var min = Mathf.RoundToInt(s.MinDay * handicap);
        var max = Mathf.RoundToInt(s.MaxDay * handicap);

        return Random.Range(min, max + 1);
    }

}
