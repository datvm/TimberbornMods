
namespace ModdableWeather.Defaults;

public class NoneTemperateWeather : IModdedTemperateWeather
{
    public const string WeatherId = nameof(NoneTemperateWeather);

    public string Id { get; } = WeatherId;
    public ModdedWeatherSpec Spec { get; } = new() { Id = WeatherId, };
    public bool Enabled { get; } = false;
    public bool Active { get; }

    public event WeatherChangedEventHandler? OnWeatherActiveChanged;

    public void Start(bool onLoad) { }
    public void End() { }

    public int GetChance(int cycle, ModdableWeatherHistoryProvider history) => 0;
    public int GetDurationAtCycle(int cycle, ModdableWeatherHistoryProvider history) => 0;


}
