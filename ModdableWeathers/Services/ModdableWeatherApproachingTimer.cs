namespace ModdableWeathers.Services;

[ReplaceSingleton]
[BypassMethods([
    nameof(OnCycleDayStarted),
])]
public class ModdableWeatherApproachingTimer(
    WeatherCycleService weatherCycleService,

    EventBus eventBus, WeatherService weatherService, GameCycleService gameCycleService, ISpecService specService)
    : HazardousWeatherApproachingTimer(eventBus, weatherService, gameCycleService, specService)
    , ILoadableSingleton, IPostLoadableSingleton
{
    readonly EventBus eb = eventBus;

    readonly List<ModdableWeatherApproachingDaysModifier> modifiers = [];
    KeyValuePair<int, int> notificationSentDay;

    public int ApproachingNotificationDays { get; private set; }

    [ReplaceProperty]
    bool MTooCloseToNotify => weatherCycleService.PartialDaysUntilNextStage < _spec.MaxDayProgressLeftToNotify;

    [ReplaceProperty(nameof(DaysToHazardousWeather))]
    public float DaysToNextWeather => weatherCycleService.PartialDaysUntilNextStage;

    void ILoadableSingleton.Load()
    {
        Load(); // call base method
        OnModifierChanged();
    }

    void IPostLoadableSingleton.PostLoad()
    {
        if (GetNextWeatherWarningProgress() > 0 && !MTooCloseToNotify)
        {
            Notify();
        }
    }

    public void AddModifier(ModdableWeatherApproachingDaysModifier modifier)
    {
        var insertionIndex = -1;

        for (int i = 0; i < modifiers.Count; i++)
        {
            var m = modifiers[i];
            if (m.Id == modifier.Id)
            {
                modifiers.RemoveAt(i);
                i--;
            }
            else if (insertionIndex == -1 && m.Order >= modifier.Order)
            {
                insertionIndex = i;
            }
        }

        if (insertionIndex == -1)
        {
            insertionIndex = modifiers.Count;
        }

        modifiers.Insert(insertionIndex, modifier);

        OnModifierChanged();
    }

    public void RemoveModifier(string id)
    {
        for (int i = 0; i < modifiers.Count; i++)
        {
            if (modifiers[i].Id != id) { continue; }

            modifiers.RemoveAt(i);
            OnModifierChanged();
            return;
        }
    }

    void OnModifierChanged()
    {
        float days = _spec.ApproachingNotificationDays;

        if (modifiers.Count > 0)
        {
            foreach (var m in modifiers)
            {
                days = m.Modify(days);
            }
        }

        ApproachingNotificationDays = Mathf.FloorToInt(days);
        CheckForNotification();
    }

    [OnEvent]
    public void OnWeatherCycleDayStartedEvent(WeatherCycleDayStartedEvent _)
        => CheckForNotification();

    void CheckForNotification()
    {
        if ((_gameCycleService.Cycle == notificationSentDay.Key && _gameCycleService.CycleDay == notificationSentDay.Value)
            || weatherCycleService.DaysUntilNextStage != ApproachingNotificationDays)
        {
            return;
        }
        Notify();
    }

    void Notify()
    {
        notificationSentDay = new(_gameCycleService.Cycle, _gameCycleService.CycleDay);

        // Do not warn if it's on the first day of the stage
        if (weatherCycleService.DaysSinceCurrentStage == 0) { return; }

        var next = weatherCycleService.NextStage;
        if (next.Stage.Weather.IsHazardous)
        {
            eb.Post(new HazardousWeatherApproachingEvent());
        }

        eb.Post(new ModdableWeatherApproachingEvent(weatherCycleService.CurrentStage, next));
    }

    [ReplaceMethod(nameof(GetProgress))]
    public float GetNextWeatherWarningProgress() 
        => 1f - weatherCycleService.PartialDaysUntilNextStage / ApproachingNotificationDays;
}

public record ModdableWeatherApproachingDaysModifier(string Id, int Order, Func<float, float> Modify);

public readonly record struct ModdableWeatherApproachingEvent(DetailedWeatherStageReference CurrentStage, DetailedWeatherStageReference NextStage);