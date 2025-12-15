namespace ModdableWeathers.Weathers.Settings;

public abstract class DefaultModdableWeatherWithSettings<TSetting>(
    ModdableWeatherSpecService specs,
    ModdableWeatherSettingsService settingsService
) : ModdableWeatherBase(specs), IModdableWeatherWithSettings<TSetting> where TSetting : IDefaultModdableWeatherSettings, new()
{

    public TSetting Settings { get; protected set; } = default!;

    public override void Load()
    {
        base.Load();
        Settings = settingsService.GetSettings<TSetting>();
    }

    public override int GetChance(int cycle, WeatherHistoryService history) => Settings.Chance;

    public override int GetDurationAtCycle(int cycle, WeatherHistoryService history)
    {
        var s = Settings;
        var occured = history.GetOccurenceCount(Id);
        var handicap = ModdableWeathersUtils.CalculateHandicap(
            occured,
            s.HandicapCycles,
            () => s.HandicapPercent
        );


        var min = Mathf.RoundToInt(s.MinDay * handicap);
        var max = Mathf.RoundToInt(s.MaxDay * handicap);

        return Random.Range(min, max + 1);
    }

}
