namespace ModdableWeather.UI;

public class ModdableHazardousWeatherApproachingTimer(
    EventBus eb,
    ModdableWeatherService weatherService,
    GameCycleService gameCycleService
) : HazardousWeatherApproachingTimer(eb, weatherService, gameCycleService),
    ILoadableSingleton, IUnloadableSingleton
{
    public const int DefaultApproachingNotificationDays = 3;
    public const float DefaultMaxDayProgressLeftToNotify = .15f;

    static ModdableHazardousWeatherApproachingTimer? instance;
    public static ModdableHazardousWeatherApproachingTimer Instance => instance.InstanceOrThrow();

    readonly List<IHazardousWeatherApproachingTimerModifier> modifiers = [];

    readonly EventBus eb = eb;
    readonly ModdableWeatherService weatherService = weatherService;

    public int LastCycleWarned { get; private set; } = 0;

    public new int ApproachingNotificationDays
    {
        get
        {
            var days = DefaultApproachingNotificationDays;
            
            if (modifiers.Count > 0)
            {
                for (int i = 0; i < modifiers.Count; i++)
                {
                    days = modifiers[i].Modify(days, DefaultApproachingNotificationDays);
                }
            }

            return days;
        }
    }

    public new float MaxDayProgressLeftToNotify { get; private set; } = DefaultMaxDayProgressLeftToNotify;

    public new bool TooCloseToNotify => DaysToHazardousWeather < MaxDayProgressLeftToNotify;

    public int WarningDay => _weatherService.HazardousWeatherStartCycleDay - ApproachingNotificationDays;
    public int DaysUntilWarning => WarningDay - _gameCycleService.CycleDay;

    public new void Load()
    {
        instance = this;
        base.Load();
    }

    public void Unload()
    {
        instance = null;
    }

    public void RegisterModifier(IHazardousWeatherApproachingTimerModifier modifier)
    {
        if (modifiers.Count == 0)
        {
            modifiers.Add(modifier);
        }
        else
        {
            var insertIndex = modifiers.FindIndex(m => m.Order > modifier.Order);

            if (insertIndex < 0)
            {
                modifiers.Add(modifier);
            }
            else
            {
                modifiers.Insert(insertIndex, modifier);
            }
        }

        RaiseWeatherChanged();
        
    }

    public void UnregisterModifier(IHazardousWeatherApproachingTimerModifier modifier)
    {
        modifiers.Remove(modifier);
        RaiseWeatherChanged();
    }

    void RaiseWeatherChanged()
    {
        eb.Post(new OnModdableWeatherChangedMidCycle(weatherService.WeatherCycleDetails));
    }

    public new float GetProgress()
    {
        return _weatherService.HazardousWeatherDuration <= 0
            ? 0f
            : 1f - DaysToHazardousWeather / ApproachingNotificationDays;
    }

    public new void OnCycleDayStarted(CycleDayStartedEvent _)
    {
        CheckForNotification();
    }

    [OnEvent]
    public void OnWeatherChanged(OnModdableWeatherChangedMidCycle _)
    {
        CheckForNotification();
    }

    public void CheckForNotification()
    {
        var cycleDay = _gameCycleService.CycleDay;
        var warningDay = WarningDay;

        if (cycleDay < warningDay
            || weatherService.IsHazardousWeather) { return; }

        var cycle = _gameCycleService.Cycle;

        if (cycleDay == warningDay
            || LastCycleWarned < cycle)
        {
            LastCycleWarned = cycle;
            NotifyHazardousWeatherApproaching();
        }
    }

}
