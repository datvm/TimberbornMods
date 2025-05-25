namespace ModdableWeather.Services;

/// <summary>
/// Provides historical weather cycle data and manages persistence for moddable weather cycles.
/// </summary>
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

    /// <summary>
    /// Gets the list of all historical weather cycles.
    /// </summary>
    public IReadOnlyList<ModdableWeatherCycle> Cycles => cycles;

    /// <summary>
    /// Gets the most recent weather cycle, or null if none exist.
    /// </summary>
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

    /// <summary>
    /// Gets the number of cycles a specific weather has occurred.
    /// </summary>
    /// <param name="weatherId">The weather ID.</param>
    /// <returns>The count of cycles for the specified weather.</returns>
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
        if (hazardousWeatherHistory._history.Count == 0) // New game
        {
            return;
        }

        cycles = [];
        var counter = 0;
        foreach (var item in hazardousWeatherHistory._history)
        {
            cycles.Add(new(
                counter++,
                new(gameTemperateWeather.Id, 0),
                new(item.HazardousWeatherId, item.Duration)));
        }

        // Load current cycle temperate weather
        // Do not use from the TemperateWeatherDurationService as it may be patched later and cause infinite recursion
        if (loader.TryGetSingleton(TemperateWeatherDurationService.TemperateWeatherDurationServiceKey, out var s)
            && s.Has(TemperateWeatherDurationService.TemperateWeatherDurationKey))
        {
            cycles[^1] = cycles[^1] with
            {
                TemperateWeather = new(
                    gameTemperateWeather.Id,
                    s.Get(TemperateWeatherDurationService.TemperateWeatherDurationKey)),
            };
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
