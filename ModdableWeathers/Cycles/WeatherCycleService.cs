namespace ModdableWeathers.Cycles;

public class WeatherCycleService : ILoadableSingleton
{
    readonly EventBus eb;
    readonly WeatherHistoryService history;
    readonly GameCycleService gameCycleService;
    readonly WeatherGenerator weatherGenerator;
    readonly IDayNightCycle dayNightCycle;
    readonly CycleDurationService cycleDurationService;

    public WeatherCycleService(
        EventBus eb,
        WeatherHistoryService history,
        GameCycleService gameCycleService,
        WeatherGenerator weatherGenerator,
        IDayNightCycle dayNightCycle,
        CycleDurationService cycleDurationService
    )
    {
        this.eb = eb;
        this.history = history;
        this.gameCycleService = gameCycleService;
        this.weatherGenerator = weatherGenerator;
        this.dayNightCycle = dayNightCycle;
        this.cycleDurationService = cycleDurationService;
        history.WeatherCycleReferenceUpdated += OnWeatherCycleReferenceUpdated;
    }

    public void Load()
    {
        var cycle = gameCycleService.Cycle;
        weatherGenerator.EnsureWeatherGenerated(cycle);
        history.UpdateReferences(cycle);

        cycleDurationService.SetForCycle(cycle);
        gameCycleService._cycleDurationInDays = cycleDurationService.DurationInDays;

        OnCycleDayStarted(null!);

        eb.Register(this);
    }

    DetailedWeatherStageReference activeStage;
    public DetailedWeatherStageReference CurrentStage { get; private set; }
    public int CurrentStageTotalDays => CurrentStage.Stage.Days;
    public int DaysSinceCurrentStage { get; private set; }
    public int CurrentStageStartDay { get; private set; }
    public IModdableWeather CurrentWeather => CurrentStage.Stage.Weather;
    public ImmutableArray<IModdableWeatherModifier> CurrentWeatherModifiers => CurrentStage.Stage.WeatherModifiers;

    public DetailedWeatherStageReference NextStage { get; private set; }
    public int DaysUntilNextStage => CurrentStageTotalDays - DaysSinceCurrentStage;
    public float PartialDaysUntilNextStage => CurrentStageTotalDays - DaysSinceCurrentStage - dayNightCycle.DayProgress;

    public DetailedWeatherStageReference TomorrowStage =>
        DaysSinceCurrentStage < CurrentStageTotalDays ? CurrentStage : NextStage;

    public DetailedWeatherStageReference? GetCurrentOrNextHazardousWeatherInThisCycle()
        => GetCurrentOrNextWeatherInThisCycle(false);
    public DetailedWeatherStageReference? GetNextHazardousWeatherInThisCycle()
        => GetNextWeatherInCycle(false);
    public DetailedWeatherStageReference? GetCurrentOrNextBenignWeatherInThisCycle()
        => GetCurrentOrNextWeatherInThisCycle(true);
    public DetailedWeatherStageReference? GetNextBenignWeatherInThisCycle()
        => GetNextWeatherInCycle(true);

    public DetailedWeatherStageReference? GetCurrentOrNextWeatherInThisCycle(bool benign)
    {
        var currWeather = CurrentWeather;
        return ((currWeather.IsBenign && benign) || (currWeather.IsHazardous && !benign))
            ? CurrentStage
            : GetNextWeatherInCycle(benign);
    }

    public DetailedWeatherStageReference? GetNextWeatherInCycle(bool benign)
    {
        var stage = CurrentStage;
        var cycle = stage.Cycle;

        for (int i = stage.StageIndex + 1; i < cycle.Stages.Length; i++)
        {
            var s = cycle.Stages[i];
            if (stage.Stage.Days > 0 && ((benign && s.Weather.IsBenign) || (!benign && s.Weather.IsHazardous)))
            {
                return new(cycle, s);
            }
        }

        return null;
    }

    void OnWeatherCycleReferenceUpdated(DetailedWeatherCycle cycle, int currentCycle) => UpdateReferences();

    void UpdateReferences()
    {
        var currCycle = history.CurrentCycle;
        if (currCycle.Stages.Length == 0)
        {
            throw new InvalidOperationException("Weather cycle has no stages.");
        }

        var day = gameCycleService.CycleDay;

        int index, startDay = 0, endDay = 0;
        for (index = 0; index < currCycle.Stages.Length; index++)
        {
            startDay = endDay + 1;
            endDay = startDay + currCycle.Stages[index].Days - 1;

            if (endDay >= day)
            {
                break;
            }
        }

        if (index >= currCycle.Stages.Length)
        {
            index = currCycle.Stages.Length - 1; // Should not happen, but just in case
        }

        CurrentStage = currCycle.GetStage(index);
        CurrentStageStartDay = startDay;
        DaysSinceCurrentStage = day - startDay;

        // Determine next stage: in the same cycle or the first stage of the next cycle
        if (index + 1 < currCycle.Stages.Length)
        {
            NextStage = currCycle.GetStage(index + 1);
        }
        else
        {
            NextStage = history.NextCycle.GetStage(0);
        }
    }

    [OnEvent]
    public void OnCycleDayStarted(CycleDayStartedEvent _)
    {
        var cycle = gameCycleService.Cycle;
        var day = gameCycleService.CycleDay;

        history.UpdateReferences(cycle, activeStage == default);
        UpdateReferences();

        var currStage = CurrentStage;
        if (activeStage != currStage)
        {
            TransitionWeather(activeStage, currStage, false);
        }

        // No matter what, update the days
        DaysSinceCurrentStage = day - CurrentStageStartDay;

        eb.Post(new WeatherCycleDayStartedEvent(
            currStage,
            DaysSinceCurrentStage,
            DaysUntilNextStage
        ));
    }

    void TransitionWeather(DetailedWeatherStageReference? prev, DetailedWeatherStageReference curr, bool onLoad)
    {
        var prevStage = prev?.Stage;

        if (prevStage is not null)
        {
            ModdableWeathersUtils.LogVerbose(() => $"Ending previous weather: {prevStage.Weather} with modifiers: {string.Join(", ", prevStage.WeatherModifiers)}");

            foreach (var m in prevStage.WeatherModifiers)
            {
                m.End();
            }
            prevStage.Weather.End();
        }

        var currStage = curr.Stage;
        ModdableWeathersUtils.LogVerbose(() => $"""        
        Starting new weather stage: Cycle {curr.Cycle.Cycle}, stage {currStage.Index}, lasting {currStage.Days} days
        - Weather: {currStage.Weather}
        - Modifiers: {string.Join(", ", currStage.WeatherModifiers)}
        """, "| ");
        currStage.Weather.Start(curr, onLoad);
        foreach (var m in currStage.WeatherModifiers)
        {
            m.Start(curr, history, onLoad);
        }
        activeStage = curr;

        eb.Post(new WeatherTransitionedEvent(
            prev,
            curr
        ));
    }

}

public readonly record struct WeatherTransitionedEvent(DetailedWeatherStageReference? From, DetailedWeatherStageReference To);
public readonly record struct WeatherCycleDayStartedEvent(DetailedWeatherStageReference Stage, int StageDay, int DaysUntilNext);