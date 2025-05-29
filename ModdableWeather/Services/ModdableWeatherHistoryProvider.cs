namespace ModdableWeather.Services;

/// <summary>
/// Provides historical weather cycle data and manages persistence for moddable weather cycles.
/// </summary>
public class ModdableWeatherHistoryProvider(
    ISingletonLoader loader,
    ModdableWeatherRegistry registry,
    EventBus eb,
    HazardousWeatherHistory hazardousWeatherHistory,
    GameTemperateWeather gameTemperateWeather,
    ModdableWeatherGenerator generator
) : ISaveableSingleton, ILoadableSingleton
{
    static readonly ListKey<ModdableWeatherCycle> CyclesKey = new("HistoricalCycles");
    static readonly PropertyKey<string> NextTemperateWeatherIdKey = new("NextTemperateWeatherId");

    List<ModdableWeatherCycle> cycles = [];
    Dictionary<string, int> counters = [];
    IModdedTemperateWeather? nextCycleTemperate;

    /// <summary>
    /// Gets the list of all historical weather cycles.
    /// </summary>
    public IReadOnlyList<ModdableWeatherCycle> Cycles => cycles;

    /// <summary>
    /// Gets the current weather cycle.
    /// </summary>
    public ModdableWeatherCycle CurrentCycle => CurrentCycleDetails.Cycle;

    ModdableWeatherCycleDetails? currentCycleDetails;
    public ModdableWeatherCycleDetails CurrentCycleDetails => currentCycleDetails ?? throw new InvalidOperationException("No weather cycles available.");

    /// <summary>
    /// Gets the current temperate weather.
    /// </summary>
    public IModdedTemperateWeather CurrentTemperateWeather => CurrentCycleDetails.TemperateWeather;

    /// <summary>
    /// Gets the current hazardous weather.
    /// </summary>
    public IModdedHazardousWeather CurrentHazardousWeather => CurrentCycleDetails.HazardousWeather;

    public bool HasNextCycleTemperateWeather => nextCycleTemperate is not null;
    public IModdedTemperateWeather NextCycleTemperateWeather => nextCycleTemperate.InstanceOrThrow();

    public void Load()
    {
        if (!LoadSavedData())
        {
            ExtractFromCurrentData();
        }

        InitData();
    }

    /// <summary>
    /// Gets the number of cycles a specific weather has occurred.
    /// </summary>
    /// <param name="weatherId">The weather ID.</param>
    /// <returns>The count of cycles for the specified weather.</returns>
    public int GetWeatherCycleCount(string weatherId) => counters[weatherId];

    void InitData()
    {
        // First game
        nextCycleTemperate ??= generator.DecideTemperateWeatherForCycle(Cycles.Count + 1, this);

        var info = cycles.LastOrDefault();
        currentCycleDetails = GetDetails(info);

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

            if (s.Has(NextTemperateWeatherIdKey))
            {
                nextCycleTemperate = registry.GetTemperateWeather(s.Get(NextTemperateWeatherIdKey));
            }

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

    public void AddCycle(ModdableWeatherCycle cycle, IModdedTemperateWeather nextTemperateWeather)
    {
        cycles.Add(cycle);

        counters[cycle.TemperateWeather.Id]++;
        counters[cycle.HazardousWeather.Id]++;

        var details = GetDetails(cycle);
        currentCycleDetails = details;
        nextCycleTemperate = nextTemperateWeather;

        eb.Post(new OnModdableWeatherCycleDecided(details));
    }

    public void Save(ISingletonSaver singletonSaver)
    {
        var s = singletonSaver.GetSingleton(ModdableWeatherUtils.SaveKey);
        s.Set(CyclesKey, cycles, ModdableWeatherCycleSerializer.Instance);
        s.Set(NextTemperateWeatherIdKey, NextCycleTemperateWeather.Id);
    }

    ModdableWeatherCycleDetails GetDetails(ModdableWeatherCycle cycle) =>
        new(cycle,
            registry.GetTemperateWeather(cycle.TemperateWeather.Id),
            registry.GetHazardousWeather(cycle.HazardousWeather.Id));

}
