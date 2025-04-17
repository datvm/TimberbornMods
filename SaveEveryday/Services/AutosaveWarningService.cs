global using Timberborn.QuickNotificationSystem;
global using Timberborn.TickSystem;
global using Timberborn.TimeSystem;

namespace SaveEveryday.Services;
public class AutosaveWarningService(
    ModSettings s,
    QuickNotificationService notf,
    SaveEverydayService saveEveryday,
    IDayNightCycle cycle,
    ILoc t
) : ITickableSingleton, ILoadableSingleton
{
    const float WarningTime = 24f - 3f;

    bool enabled;
    bool warned;

    public void Load()
    {
        s.ModSettingChanged += (_, _) =>
        {
            enabled = s.AutosaveWarning;
        };
        enabled = s.Enabled && s.AutosaveWarning;
    }

    public void Tick()
    {
        if (!enabled) { return; }

        if (cycle.HoursPassedToday < WarningTime)
        {
            warned = false;
        }
        else if (!warned)
        {
            warned = true;

            if (saveEveryday.WillSaveNextDay)
            {
                SendNotification();
            }
        }
    }

    void SendNotification()
    {
        warned = true;
        notf.SendWarningNotification(t.T("LV.SE.AutosaveWarningText", 24f - WarningTime));
    }

}