namespace ModdableWeather.Services;

public class ModdableWeatherHistoryProvider(
    ISingletonLoader loader,
    ModdableWeatherRegistry registry,
    EventBus eb
) : ISaveableSingleton, ILoadableSingleton
{
    static readonly ListKey<ModdableWeatherCycle> CyclesKey = new("HistoricalCycles");

    List<ModdableWeatherCycle> cycles = [];
    Dictionary<string, int> counters = [];

    public IReadOnlyList<ModdableWeatherCycle> Cycles => cycles;

    public void Load()
    {
        LoadSavedData();
        InitCounters();
        eb.Register(this);
    }

    void InitCounters()
    {
        counters = registry.AllWeathers.ToDictionary(q => q.Id, _ => 0);

        foreach (var cycle in cycles)
        {
            counters[cycle.TemperateWeather.Id]++;
            counters[cycle.HazardousWeather.Id]++;
        }
    }

    void LoadSavedData()
    {
        if (!loader.TryGetSingleton(ModdableWeatherUtils.SaveKey, out var s)) { return; }

        if (s.Has(CyclesKey))
        {
            cycles = s.Get(CyclesKey, ModdableWeatherCycleSerializer.Instance);
        }
    }

    [OnEvent]
    public void OnModdableWeatherCycleDecided(OnModdableWeatherCycleDecided ev)
    {
        var c = ev.WeatherCycle;
        cycles.Add(c);

        counters[c.TemperateWeather.Id]++;
        counters[c.HazardousWeather.Id]++;
    }

    public void Save(ISingletonSaver singletonSaver)
    {
        var s = singletonSaver.GetSingleton(ModdableWeatherUtils.SaveKey);
        s.Set(CyclesKey, cycles, ModdableWeatherCycleSerializer.Instance);
    }

}
