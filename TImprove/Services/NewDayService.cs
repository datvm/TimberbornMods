namespace TImprove.Services;

public class NewDayService(
    EventBus eb,
    MSettingsNewDay s,
    GameCycleService gameCycleService,
    WeatherService weatherService,
    SpeedManager speedManager
) : ILoadableSingleton
{

    public void Load()
    {
        eb.Register(this);
    }

    [OnEvent]
    public void OnNewCycleDay(CycleDayStartedEvent _)
    {
        if (!s.HasAction || !ShouldDoAction()) { return; }

        DoAction();
    }

    [OnEvent]
    public void OnHazardousWeatherApproachingEvent(HazardousWeatherApproachingEvent _)
    {
        if (!s.HasAction || s.ActionOnAllDays || !s.NewDayHazard.Value) { return; }

        DoAction();
    }

    void DoAction()
    {
        var action = s.NewDayActionValue;

        switch (action)
        {
            case NewDayActionValue.Pause:
                speedManager.ChangeSpeed(0f);
                break;
            case NewDayActionValue.Speed1:
                speedManager.ChangeSpeed(1f);
                break;
        }
    }

    bool ShouldDoAction()
    {
        if (s.ActionOnAllDays) { return true; }

        var cycleDay = gameCycleService.CycleDay;
        if (s.NewDayNewCycle.Value && cycleDay == 1) { return true; }
        if (s.NewDayHazard.Value && cycleDay == weatherService.HazardousWeatherStartCycleDay) { return true; }

        return false;
    }

}
