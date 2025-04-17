global using Timberborn.QuickNotificationSystem;
global using Timberborn.TickSystem;

namespace ConfigurableAutoSave.Services;
public class AutosaveWarningService(MSettings s, QuickNotificationService notf, Autosaver autosaver, ILoc t) : ITickableSingleton, ILoadableSingleton
{
    const float WarningTime = 5f;

    bool enabled;
    bool warned;

    public void Load()
    {
        s.OnSettingsChanged += () =>
        {
            enabled = s.AutosaveWarning;
        };
        enabled = s.AutosaveWarning;
    }

    public void Tick()
    {
        if (!(enabled && autosaver.IsNotBlocked())) { return; }

        var next = autosaver._nextSaveTime - Time.unscaledTime;
        if (next > WarningTime)
        {
            warned = false;
            return;
        }
        else if (!warned)
        {
            warned = true;
            notf.SendWarningNotification(t.T("LV.CAS.AutosaveWarningText", WarningTime));
        }
    }

}