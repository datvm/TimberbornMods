namespace ModdableWeathers.Historical;

public class WeatherHistoryService(
    WeatherHistoryRegistry history,
    ModdableWeatherRegistry weathers,
    GameCycleService gameCycleService,
    ModdableWeatherModifierRegistry weatherModifiers
) : ILoadableSingleton
{

    public DetailedWeatherCycle? PreviousCycle { get; private set; } = null!;
    public DetailedWeatherCycle CurrentCycle { get; private set; } = null!;
    public DetailedWeatherCycle NextCycle { get; private set; } = null!;

    readonly Dictionary<string, int> weatherCounters = [];
    int cycleCounted = 0;

    public event Action<DetailedWeatherCycle>? WeatherCycleAdded;

    public void Load()
    {
        history.WeatherCycleAdded += OnWeatherCycleAdded;
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

    public int GetOccurrenceCount(string weatherId) => weatherCounters.GetValueOrDefault(weatherId);

    public void UpdateReferences()
    {
        var currCycle = gameCycleService.Cycle;

        PreviousCycle = currCycle > 1 ? GetWeatherCycle(currCycle - 1) : null;
        CurrentCycle = GetWeatherCycle(currCycle);
        NextCycle = GetWeatherCycle(currCycle + 1);

        UpdateCounters();
    }

    void UpdateCounters()
    {
        var target = gameCycleService.Cycle;
        if (cycleCounted >= target) { return; }

        for (var c = cycleCounted + 1; c <= target; c++)
        {
            var weatherCycle = history[c];

            foreach (var stage in weatherCycle.Stages)
            {
                weatherCounters[stage.WeatherId] = weatherCounters.GetValueOrDefault(stage.WeatherId) + 1;
            }
        }

        cycleCounted = target;
    }

    void OnWeatherCycleAdded(WeatherCycle _)
    {
        UpdateReferences();
        WeatherCycleAdded?.Invoke(CurrentCycle);
    }
}
