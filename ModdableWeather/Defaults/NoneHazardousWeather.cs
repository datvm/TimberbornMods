

namespace ModdableWeather.Defaults;

public class NoneHazardousWeather : IModdedHazardousWeather
{

    public string Id { get; } = nameof(NoneHazardousWeather);
    public ModdedWeatherSpec Spec { get; } = new() { Id = nameof(NoneHazardousWeather), };

    public bool Active { get; }

    public event WeatherChangedEventHandler? OnWeatherActiveChanged;

    public int GetDurationAtCycle(int cycle, ModdableWeatherHistoryProvider history)
        => 0;

    public int GetChance(int cycle, ModdableWeatherHistoryProvider history)
        => 100;

    public void Start(bool onLoad)
    {
    }

    public void End()
    {
    }

    public int GetDurationAtCycle(int cycle) => 0;
}
