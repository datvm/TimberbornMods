namespace ModdableWeathers.Historical;

public class WeatherHistoryService(
    WeatherHistoryRegistry history,
    ModdableWeatherRegistry weathers,
    ModdableWeatherModifierRegistry weatherModifiers
) : ILoadableSingleton
{

    public DetailedWeatherCycle? PreviousCycle { get; private set; } = null!;
    public DetailedWeatherCycle CurrentCycle { get; private set; } = null!;
    public DetailedWeatherCycle NextCycle { get; private set; } = null!;

    readonly Dictionary<string, int> weatherCounters = [];
    readonly Dictionary<string, int> weatherModifierCounters = [];
    int cycleCounted = 0;

    public event DetailedWeatherCycleHandler? WeatherCycleAdded;
    public event DetailedWeatherCycleHandler WeatherCycleReferenceUpdated = null!;

    public void Load()
    {
        history.WeatherCycleAdded += RegistryWeatherCycleAdded;
    }

    void RegistryWeatherCycleAdded(WeatherCycle cycle, int currentCycle)
    {
        UpdateReferences(currentCycle);
        WeatherCycleAdded?.Invoke(GetWeatherCycle(cycle.Cycle), currentCycle);
    }

    public DetailedWeatherCycle GetWeatherCycle(int cycleIndex)
    {
        var cycle = history[cycleIndex];

        return new(cycle, [..cycle.Stages.Select(s => new DetailedWeatherCycleStage(
            s.Index,
            s.IsBenign,
            weathers.GetOrFallback(s.WeatherId, s.IsBenign),
            [..s.WeatherModifierIds.Select(id => weatherModifiers.GetOrFallback(id)).Where(static q => q is not null)!],
            s.Days
        ))]);
    }

    public int GetWeatherOccurrenceCount(string weatherId) => weatherCounters.GetValueOrDefault(weatherId);
    public int GetWeatherModifierOccurrenceCount(string weatherModifierId) => weatherModifierCounters.GetValueOrDefault(weatherModifierId);

    public void UpdateReferences(int currCycle, bool force = false)
    {
        if (!force && CurrentCycle?.Cycle == currCycle) { return; }

        PreviousCycle = currCycle > 1 ? GetWeatherCycle(currCycle - 1) : null;
        CurrentCycle = GetWeatherCycle(currCycle);
        NextCycle = GetWeatherCycle(currCycle + 1);

        UpdateCounters(currCycle);

        WeatherCycleReferenceUpdated(CurrentCycle, currCycle);
    }

    void UpdateCounters(int target)
    {
        if (cycleCounted >= target) { return; }

        for (var c = cycleCounted + 1; c <= target; c++)
        {
            var weatherCycle = history[c];

            foreach (var stage in weatherCycle.Stages)
            {
                weatherCounters[stage.WeatherId] = weatherCounters.GetValueOrDefault(stage.WeatherId) + 1;

                foreach (var m in stage.WeatherModifierIds)
                {
                    weatherModifierCounters[m] = weatherModifierCounters.GetValueOrDefault(m) + 1;
                }
            }
        }

        cycleCounted = target;
    }

}

public delegate void DetailedWeatherCycleHandler(DetailedWeatherCycle cycle, int currentCycle);