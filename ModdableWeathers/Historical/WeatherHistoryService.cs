namespace ModdableWeathers.Historical;

public class WeatherHistoryService(
    WeatherHistoryRegistry history,
    ModdableWeatherRegistry weathers,
    GameCycleService gameCycleService
) : ILoadableSingleton
{

    public DetailedWeatherCycle? PreviousCycle { get; private set; } = null!;
    public DetailedWeatherCycle CurrentCycle { get; private set; } = null!;
    public DetailedWeatherCycle NextCycle { get; private set; } = null!;

    readonly Dictionary<string, int> weatherCounters = [];
    int cycleCounted = -1;

    public void Load()
    {
        history.WeatherCycleAdded += OnWeatherCycleAdded;
        UpdateReferences();
    }

    public DetailedWeatherCycle GetWeatherCycle(int index)
    {
        var cycle = history[index];

        return new(cycle, [..cycle.Stages.Select(s => new DetailedWeatherCycleStage(
            s.Index,
            s.IsBenign,
            weathers.GetOrFallback(s.WeatherId, s.IsBenign),
            s.Days
        ))]);
    }

    public int GetOccurenceCount(string weatherId) => weatherCounters.GetValueOrDefault(weatherId);

    void UpdateReferences()
    {
        var currCycle = gameCycleService.Cycle;

        PreviousCycle = currCycle > 0 ? GetWeatherCycle(currCycle - 1) : null;
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

    void OnWeatherCycleAdded(WeatherCycle _) => UpdateReferences();

}
