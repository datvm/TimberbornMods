namespace ModdableWeather.Weathers;

public abstract class ModdableWeatherWithSettings<TSettings>(
    WeatherEntitySettingEntry weatherSettingsService
) : ModdableWeatherBase, ILoadableSingleton, IWeatherWithSettings<TSettings>
    where TSettings : WeatherSettingEntry
{
    public TSettings Settings { get; protected set; } = null!;

    public virtual void Load()
    {
        var entry = Settings = weatherSettingsService.Get(this);
        Enabled = entry.Enabled;
    }

    public override int GetChance(int cycle, ModdableWeatherHistoryProvider history)
        => cycle < Settings.StartCycle ? 0 : Settings.Chance;

    public override int GetDurationAtCycle(int cycle, ModdableWeatherHistoryProvider history)
    {
        var s = Settings;
        var handicap = ModdableWeatherUtils.CalculateHandicap(cycle, s.HandicapCycles, () => s.HandicapPerc);

        var min = Mathf.RoundToInt(s.MinDay * handicap);
        var max = Mathf.RoundToInt(s.MaxDay * handicap);

        return Random.Range(min, max + 1);
    }

}
