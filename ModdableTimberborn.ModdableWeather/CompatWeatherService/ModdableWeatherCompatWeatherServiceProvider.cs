namespace ModdableTimberborn.ModdableWeather.CompatWeatherService;

[MultiBind(typeof(ICompatWeatherServiceProvider))]
public class ModdableWeatherCompatWeatherServiceProvider(
    ModdableWeatherRegistry registry,
    ModdableWeatherApproachingTimer approachingTimer,
    WeatherCycleService weatherCycleService
) : ICompatWeatherServiceProvider, ILoadableSingleton
{

    ImmutableArray<CompatWeatherType> allWeathers = [];

    public IReadOnlyList<CompatWeatherType> WeatherTypes => allWeathers;
    public int Priority => 100;
    public int ApproachingNotificationDays => approachingTimer.ApproachingNotificationDays;
    public string? CurrentCycleHazardousId => weatherCycleService.GetCurrentOrNextHazardousWeatherInThisCycle()?.Weather.Id;
    public bool IsHazardous => weatherCycleService.CurrentWeather.IsHazardous;
    public bool IsNextDayHazardous => weatherCycleService.TomorrowStage.Weather.IsHazardous;

    public void Load()
    {
        allWeathers = [..registry.Weathers.Select(w => new CompatWeatherType(w.Id, w.Spec.DisplayLoc, w.IsBenign))];
    }

    public CompatWeatherCycle GetCurrentCycle()
    {
        var (cycle, stage) = weatherCycleService.CurrentStage;

        var c = cycle.Cycle;
        var day = 1;
        return new(c, [..cycle.Stages.Select(s => {
            var d = day;
            day += s.Days;
            return new CompatWeatherCycleStage(c, s.Index, d, s.Days, s.Weather.Id, s.Weather.IsBenign);
        })]);
    }

    public CompatWeatherCycleStage GetCurrentCycleStage() 
        => weatherCycleService.CurrentStage.ToCompatWeatherCycleStage();

    public CompatWeatherCycleStage? GetCurrentCycleHazardousStage()
        => GetCurrencyCycleStageWithWeather(false);

    public CompatWeatherCycleStage? GetCurrentCycleBenignStage()
        => GetCurrencyCycleStageWithWeather(true);

    CompatWeatherCycleStage? GetCurrencyCycleStageWithWeather(bool benign)
    {
        var s = weatherCycleService.CurrentStage;
        var cycle = s.Cycle;
        var sCount = cycle.Stages.Length;

        // Check current
        if (Matches(s.Stage)) { return s.ToCompatWeatherCycleStage(); }

        // First move forward
        for (int i = s.StageIndex + 1; i < sCount; i++)
        {
            var curr = cycle.Stages[i];
            if (Matches(curr)) { return curr.ToCompatWeatherCycleStage(cycle); }
        }

        // Then move backward
        for (int i = s.StageIndex - 1; i >= 0; i--)
        {
            var curr = cycle.Stages[i];
            if (Matches(curr)) { return curr.ToCompatWeatherCycleStage(cycle); }
        }

        return null;

        bool Matches(DetailedWeatherCycleStage s) => s.IsBenign == benign;
    }

    public CompatNextWeatherCycleStage GetNextCycleStage()
        => weatherCycleService.NextStage.ToCompatWeatherCycleStage();

    public CompatWeatherWarning GetWarningStatus()
    {
        var warningPeriod = ApproachingNotificationDays;
        var daysUntilNext = weatherCycleService.PartialDaysUntilNextStage;
        var nextWeatherId = weatherCycleService.NextStage.Weather.Id;

        if (daysUntilNext > warningPeriod) // Not showing yet
        {
            return new(CompatWeatherWarningStage.NoWarning, daysUntilNext, nextWeatherId);
        }
        else
        {
            var showingToday = Mathf.CeilToInt(daysUntilNext) == ApproachingNotificationDays;
            return new(showingToday ? CompatWeatherWarningStage.ShowedToday : CompatWeatherWarningStage.Showing, daysUntilNext, nextWeatherId);
        }
    }

}
