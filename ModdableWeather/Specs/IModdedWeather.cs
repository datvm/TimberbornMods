
namespace ModdableWeather.Specs;

public interface IModdedWeather
{
    string Id { get; }
    ModdedWeatherSpec Spec { get; set; }

    public string WeatherId => Id;

    bool Active { get; }
    event EventHandler? OnWeatherStarted;
    event EventHandler? OnWeatherEnded;
    event EventHandler? OnWeatherActiveChanged;

    void Start();
    void End();

    public IModdedHazardousWeather? IsHazardous() => this as IModdedHazardousWeather;
    public IModdedTemperateWeather? IsTemperate() => this as IModdedTemperateWeather;

    int GetDurationAtCycle(int cycle, ModdableWeatherHistoryProvider history);
    int GetChance(int cycle, ModdableWeatherHistoryProvider history);
}
