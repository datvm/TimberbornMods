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
) : ISaveableSingleton, ILoadableSingleton, IUnloadableSingleton
{
    static ModdableWeatherHistoryProvider? instance;
    public static ModdableWeatherHistoryProvider Instance => instance.InstanceOrThrow();

    static readonly ListKey<ModdableWeatherCycle> CyclesKey = new("HistoricalCycles");
    static readonly PropertyKey<string> NextTemperateWeatherIdKey = new("NextTemperateWeatherId");
    static readonly PropertyKey<ModdableWeatherNextCycleWeather> NextCycleWeatherKey = new("NextCycleWeatherKey");

    List<ModdableWeatherCycle> cycles = [];
    Dictionary<string, int> counters = [];
    ModdableWeatherNextCycleWeather? nextCycleWeather;

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

    public bool HasNextCycleWeather => nextCycleWeather is not null;
    public ModdableWeatherNextCycleWeather NextCycleWeather => nextCycleWeather.InstanceOrThrow();
    public IModdedTemperateWeather NextCycleTemperateWeather => NextCycleWeather.TemperateWeather ?? registry.GameTemperateWeather;

    public bool PreviousCycleHadDrought
    {
        get
        {
            if (cycles.Count <= 1) { return false; }

            var lastCycle = cycles[^2];
            return lastCycle.HazardousWeather.Id == GameDroughtWeather.WeatherId;
        }
    }

    public bool IsCycleWithDrought => CurrentCycleDetails.HazardousWeather.IsDrought();

    public void Load()
    {
        instance = this;

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

    public void ChangeWeatherDuration(bool temperate, int days)
    {
        var cycle = Cycles[^1];

        if (temperate)
        {
            cycle = cycle with { TemperateWeather = cycle.TemperateWeather with { Duration = days } };
        }
        else
        {
            cycle = cycle with { HazardousWeather = cycle.HazardousWeather with { Duration = days } };
        }

        cycles[^1] = cycle;
        var details = GetDetails(cycle);
        currentCycleDetails = details;
        eb.Post(new OnModdableWeatherChangedMidCycle(details));
    }

    void InitData()
    {
        // First time with mod/new game
        nextCycleWeather ??= generator.DecideNextCycleWeather(Cycles.Count, this);

        var info = cycles.LastOrDefault();
        if (info is not null)
        {
            currentCycleDetails = GetDetails(info);
        }

        counters = registry.AllWeathers.ToDictionary(q => q.Id, _ => 0);

        foreach (var cycle in cycles)
        {
            counters[cycle.TemperateWeather.Id]++;
            counters[cycle.HazardousWeather.Id]++;
        }
    }

    bool LoadSavedData()
    {
        if (!(loader.TryGetSingleton(ModdableWeatherUtils.SaveKey, out var s)
            && s.Has(CyclesKey))) { return false; }

        cycles = s.Get(CyclesKey, ModdableWeatherJsonSerializer<ModdableWeatherCycle>.Instance);

        // Ensure the weather is valid
        if (cycles.Count > 0)
        {
            var lastCycle = cycles[^1];

            ModdableWeatherCycleWeather? temperateReplacement = registry.HasWeather(lastCycle.TemperateWeather.Id)
                ? null : new ModdableWeatherCycleWeather(
                    gameTemperateWeather.Id,
                    lastCycle.TemperateWeather.Duration);
            ModdableWeatherCycleWeather? hazReplacement = registry.HasWeather(lastCycle.HazardousWeather.Id)
                ? null : new ModdableWeatherCycleWeather(
                    registry.NoneHazardousWeather.Id,
                    lastCycle.HazardousWeather.Duration);

            if (temperateReplacement is not null || hazReplacement is not null)
            {
                cycles[^1] = lastCycle with
                {
                    TemperateWeather = temperateReplacement ?? lastCycle.TemperateWeather,
                    HazardousWeather = hazReplacement ?? lastCycle.HazardousWeather,
                };
            }
        }

        if (s.Has(NextCycleWeatherKey))
        {
            nextCycleWeather = s.Get(NextCycleWeatherKey, ModdableWeatherJsonSerializer<ModdableWeatherNextCycleWeather>.Instance);

            if (registry.HasWeather(nextCycleWeather.TemperateWeatherId))
            {
                nextCycleWeather.TemperateWeather = registry.GetTemperateWeather(nextCycleWeather.TemperateWeatherId);
            }
            else
            {
                nextCycleWeather = new(nextCycleWeather.SingleMode, nextCycleWeather.IsTemperate, gameTemperateWeather);
            }
        }
        else if (s.Has(NextTemperateWeatherIdKey)) // Compatible with previous save
        {
            var id = s.Get(NextTemperateWeatherIdKey);

            nextCycleWeather = new(false, true, id);
        }

        return true;

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

    public void AddCycle(ModdableWeatherCycle cycle, ModdableWeatherNextCycleWeather nextCycleWeather)
    {
        cycles.Add(cycle);

        counters[cycle.TemperateWeather.Id]++;
        counters[cycle.HazardousWeather.Id]++;

        var details = GetDetails(cycle);
        currentCycleDetails = details;
        this.nextCycleWeather = nextCycleWeather;

        eb.Post(new OnModdableWeatherCycleDecided(details));
    }

    public void Save(ISingletonSaver singletonSaver)
    {
        var s = singletonSaver.GetSingleton(ModdableWeatherUtils.SaveKey);
        s.Set(CyclesKey, cycles, ModdableWeatherJsonSerializer<ModdableWeatherCycle>.Instance);
        s.Set(NextCycleWeatherKey, NextCycleWeather, ModdableWeatherJsonSerializer<ModdableWeatherNextCycleWeather>.Instance);
    }

    ModdableWeatherCycleDetails GetDetails(ModdableWeatherCycle cycle) =>
        new(cycle,
            registry.GetTemperateWeather(cycle.TemperateWeather.Id),
            registry.GetHazardousWeather(cycle.HazardousWeather.Id));

    public void Unload()
    {
        instance = null;
    }
}
