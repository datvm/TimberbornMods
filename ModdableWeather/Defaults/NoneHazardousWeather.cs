namespace ModdableWeather.Defaults;

public class NoneHazardousWeather : IModdedHazardousWeather
{
    public const string WeatherId = nameof(NoneHazardousWeather);

    public string Id { get; } = WeatherId;
    public ModdedWeatherSpec Spec { get; } = new() { Id = nameof(NoneHazardousWeather), };
    public bool Enabled { get; } = false;

    public bool Active { get; private set; }

    public event WeatherChangedEventHandler? OnWeatherActiveChanged;

    public int GetDurationAtCycle(int cycle, ModdableWeatherHistoryProvider history)
        => 0;

    public int GetChance(int cycle, ModdableWeatherHistoryProvider history)
        => 100;

    public void Start(bool onLoad)
    {
        Active = true;
        OnWeatherActiveChanged?.Invoke(this, true, onLoad);
    }

    public void End()
    {
        Active = false;
        OnWeatherActiveChanged?.Invoke(this, false, false);
    }

    public int GetDurationAtCycle(int cycle) => 0;
}
