namespace TImprove.Services;

public class WorkingHoursHandler(
    WorkingHoursManager workingHoursManager,
    EventBus eb,
    MSettingsNewDay s,
    SpeedManager speedManager
) : ILoadableSingleton
{
    public void Load()
    {
        eb.Register(this);
    }

    [OnEvent]
    public void OnWorkingHourStateChanged(WorkingHoursTransitionedEvent _)
    {
        if (!workingHoursManager.AreWorkingHours && s.ShiftEndSpeed != -1)
        {
            speedManager.ChangeSpeed(s.ShiftEndSpeed);
        }
    }

}
