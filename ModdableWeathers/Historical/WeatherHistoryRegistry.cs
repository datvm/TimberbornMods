namespace ModdableWeathers.Historical;

public class WeatherHistoryRegistry(
    ISingletonLoader loader,
    HazardousWeatherHistory baseGameHistory
) : ILoadableSingleton, ISaveableSingleton
{
    static readonly SingletonKey SaveKey = new(nameof(WeatherHistoryRegistry));
    static readonly ListKey<string> WeatherCyclesKey = new("WeatherCycles");

    readonly List<WeatherCycle> weatherCycles = [WeatherCycle.Empty];
    public IReadOnlyList<WeatherCycle> WeatherCycles => weatherCycles;
    public WeatherCycle this[int index] => weatherCycles[index];
    public int CycleCount => weatherCycles.Count - 1;

    public event WeatherCycleHandler WeatherCycleAdded = null!;

    public void Load()
    {
        if (!TryLoadSavedData())
        {
            TryRestoreFromHistoricalData();
        }
    }

    bool TryLoadSavedData()
    {
        if (!loader.TryGetSingleton(SaveKey, out var s)) { return false; }

        var raw = s.Get(WeatherCyclesKey);
        weatherCycles.Clear();
        weatherCycles.AddRange(raw.Select(WeatherCycle.Deserialize));

        return true;
    }

    void TryRestoreFromHistoricalData()
    {
        var history = baseGameHistory._history;
        if (history.Count == 0) { return; } // New game

        var counter = 0;

        foreach (var c in history)
        {
            var modifier = c.HazardousWeatherId == GameDroughtWeather.WeatherId
                ? DroughtModifier.ModifierId
                : BadtideModifier.ModifierId;

            weatherCycles.Add(new(
                ++counter,
                [
                    new(0, true, GameTemperateWeather.WeatherId, [], 0),
                    new(1, false, c.HazardousWeatherId, [DroughtModifier.ModifierId], c.Duration),
                ]
            ));
        }

        // Load current cycle temperate weather
        // Do not use from the TemperateWeatherDurationService as it may be patched later and cause infinite recursion
        if (loader.TryGetSingleton(TemperateWeatherDurationService.TemperateWeatherDurationServiceKey, out var s)
            && s.Has(TemperateWeatherDurationService.TemperateWeatherDurationKey))
        {
            var curr = weatherCycles[^1];

            weatherCycles[^1] = curr with
            {
                Stages = [
                    curr.Stages[0] with { Days = s.Get(TemperateWeatherDurationService.TemperateWeatherDurationKey) },
                    curr.Stages[1],
                ],
            };
        }

        if (TimberUiUtils.HasMoreModLogs)
        {
            PrintExtractLog();
        }
    }

    void PrintExtractLog()
    {
        ModdableWeathersUtils.LogVerbose(() => $"Extracted {weatherCycles.Count} cycles from current data.");
        foreach (var cycle in weatherCycles)
        {
            ModdableWeathersUtils.LogVerbose(cycle.ToString);
        }
    }

    public void AddWeatherCycles(WeatherCycle cycle, int currCycle)
    {
        var index = cycle.Cycle;

        if (index != weatherCycles.Count)
        {
            throw new InvalidOperationException($"Expecting cycle index {weatherCycles.Count} but got {index}");
        }

        weatherCycles.Add(cycle);

        // Don't call this if it's the first entry because another entry will be added soon
        // And this mod expects at least 2 cycles to be present before notifying
        if (cycle.Cycle > 1)
        {
            WeatherCycleAdded(cycle, currCycle);
        }
    }

    public void ClearFutureEntries(int currCycle)
    {
        for (int i = weatherCycles.Count - 1; i > currCycle; i--)
        {
            weatherCycles.RemoveAt(i);
        }
    }

    public void Save(ISingletonSaver singletonSaver)
    {
        var s = singletonSaver.GetSingleton(SaveKey);
        s.Set(WeatherCyclesKey, [.. weatherCycles.Select(q => q.Serialize())]);
    }

}

public delegate void WeatherCycleHandler(WeatherCycle cycles, int currentCycle);