
namespace ModdableWeather.Specs;

public interface IModdedWeather
{
    string Id { get; }
    ModdedWeatherSpec Spec { get; }

    public string WeatherId => Id;

    bool Active { get; }
    event WeatherChangedEventHandler? OnWeatherActiveChanged;

    void Start(bool onLoad);
    void End();

    public IModdedHazardousWeather? IsHazardous() => this as IModdedHazardousWeather;
    public IModdedTemperateWeather? IsTemperate() => this as IModdedTemperateWeather;

    int GetDurationAtCycle(int cycle, ModdableWeatherHistoryProvider history);
    int GetChance(int cycle, ModdableWeatherHistoryProvider history);
}

public delegate void WeatherChangedEventHandler(IModdedWeather weather, bool active, bool onLoad);