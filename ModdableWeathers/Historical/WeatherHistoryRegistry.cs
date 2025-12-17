namespace ModdableWeathers.Historical;

public class WeatherHistoryRegistry(
    ISingletonLoader loader,
    HazardousWeatherHistory baseGameHistory
) : ILoadableSingleton, ISaveableSingleton
{
    static readonly SingletonKey SaveKey = new(nameof(WeatherHistoryRegistry));
    static readonly ListKey<string> WeatherCyclesKey = new("WeatherCycles");

    readonly List<WeatherCycle> weatherCycles = [];
    public IReadOnlyList<WeatherCycle> WeatherCycles => weatherCycles;
    public WeatherCycle this[int index] => weatherCycles[index - 1];
    public int CycleCount => weatherCycles.Count;

    public event Action<WeatherCycle> WeatherCycleAdded = null!;

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

        weatherCycles.Clear();
        var counter = 0;

        foreach (var c in history)
        {
            weatherCycles.Add(new(
                counter++,
                [
                    new(0, true, GameTemperateWeather.WeatherId, [], 0),
                    new(1, false, c.HazardousWeatherId, [], c.Duration),
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

    public void AddWeatherCycle(WeatherCycle cycle)
    {
        weatherCycles.Add(cycle);
        WeatherCycleAdded(cycle);
    }

    public void ReplaceNextCycle(WeatherCycle cycle, int cycleIndex)
    {
        cycleIndex--; // Convert to 0-based index

        if (cycleIndex >= weatherCycles.Count)
        {
            AddWeatherCycle(cycle);
        }
        else if (cycleIndex == weatherCycles.Count - 1)
        {
            weatherCycles[cycleIndex] = cycle;
            WeatherCycleAdded(cycle);
        }
        else
        {
            throw new InvalidOperationException($"Can only replace the next upcoming weather cycle: requested {cycleIndex + 1} vs {weatherCycles.Count}");
        }
    }

    public void Save(ISingletonSaver singletonSaver)
    {
        var s = singletonSaver.GetSingleton(SaveKey);
        s.Set(WeatherCyclesKey, [.. weatherCycles.Select(q => q.Serialize())]);
    }

}
