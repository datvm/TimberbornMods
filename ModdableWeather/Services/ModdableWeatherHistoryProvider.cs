namespace ModdableWeather.Services;

public class ModdableWeatherHistoryProvider(
    ISingletonLoader loader,
    ModdableWeatherRegistry registry,
    EventBus eb,
    HazardousWeatherHistory hazardousWeatherHistory,
    GameTemperateWeather gameTemperateWeather
) : ISaveableSingleton, ILoadableSingleton
{
    static readonly ListKey<ModdableWeatherCycle> CyclesKey = new("HistoricalCycles");

    List<ModdableWeatherCycle> cycles = [];
    Dictionary<string, int> counters = [];

    public IReadOnlyList<ModdableWeatherCycle> Cycles => cycles;
    public ModdableWeatherCycle? CurrentCycle { get; private set; }

    public void Load()
    {
        if (!LoadSavedData())
        {
            ExtractFromCurrentData();
        }

        InitData();
        eb.Register(this);
    }

    public int GetWeatherCycleCount(string weatherId) => counters[weatherId];

    void InitData()
    {
        CurrentCycle = cycles.LastOrDefault();
        counters = registry.AllWeathers.ToDictionary(q => q.Id, _ => 0);

        foreach (var cycle in cycles)
        {
            counters[cycle.TemperateWeather.Id]++;
            counters[cycle.HazardousWeather.Id]++;
        }
    }

    bool LoadSavedData()
    {
        if (!loader.TryGetSingleton(ModdableWeatherUtils.SaveKey, out var s)) { return false; }

        if (s.Has(CyclesKey))
        {
            cycles = s.Get(CyclesKey, ModdableWeatherCycleSerializer.Instance);
            return true;
        }
        else
        {
            return false;
        }
    }

    void ExtractFromCurrentData()
    {
        cycles = [];
        var counter = 0;
        foreach (var item in hazardousWeatherHistory._history)
        {
            cycles.Add(new(
                counter++,
                new(gameTemperateWeather.Id, 0),
                new(item.HazardousWeatherId, item.Duration)));
        }

        if (ModdableWeatherUtils.HasMoreModLog)
        {
            PrintExtractLog();
        }
    }

    void PrintExtractLog()
    {
        ModdableWeatherUtils.Log(() => $"Extracted {cycles.Count} cycles from current data.");
        foreach (var cycle in cycles)
        {
            ModdableWeatherUtils.Log(cycle.ToString);
        }
    }

    [OnEvent]
    public void OnModdableWeatherCycleDecided(OnModdableWeatherCycleDecided ev)
    {
        var c = ev.WeatherCycle;
        cycles.Add(c);

        counters[c.TemperateWeather.Id]++;
        counters[c.HazardousWeather.Id]++;

        CurrentCycle = c;
    }

    public void Save(ISingletonSaver singletonSaver)
    {
        var s = singletonSaver.GetSingleton(ModdableWeatherUtils.SaveKey);
        s.Set(CyclesKey, cycles, ModdableWeatherCycleSerializer.Instance);
    }

}
