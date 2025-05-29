namespace ModdableWeather.UI;

public class ModdableHazardousWeatherApproachingTimer(
    EventBus eventBus,
    WeatherService weatherService,
    GameCycleService gameCycleService
) : HazardousWeatherApproachingTimer(eventBus, weatherService, gameCycleService),
    ILoadableSingleton, IUnloadableSingleton
{
    public const int DefaultApproachingNotificationDays = 3;
    public const float DefaultMaxDayProgressLeftToNotify = .15f;

    static ModdableHazardousWeatherApproachingTimer? instance;
    public static ModdableHazardousWeatherApproachingTimer Instance => instance.InstanceOrThrow();

    public new int ApproachingNotificationDays { get; set; } = DefaultApproachingNotificationDays;
    public new float MaxDayProgressLeftToNotify { get; set; } = DefaultMaxDayProgressLeftToNotify;

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

    public new float GetProgress()
    {
        return _weatherService.HazardousWeatherDuration <= 0 
            ? 0f 
            : 1f - DaysToHazardousWeather / ApproachingNotificationDays;
    }

    public new void OnCycleDayStarted(CycleDayStartedEvent cycleDayStartedEvent)
    {
        if (_gameCycleService.CycleDay == WarningDay)
        {
            NotifyHazardousWeatherApproaching();
        }
    }

}
