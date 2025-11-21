namespace TImprove.UI;

public class RealTimePanel(
    WorkingHoursPanel workingHours,
    MSettings s
) : IUpdatableSingleton, ILoadableSingleton
{
    VisualElement icon = null!;
    Label lblRealTime = null!;

    bool lastEnabled;
    bool enabled, clock24;

    public void Load()
    {
        icon = workingHours._root.Q(classes: "working-hours-panel__icon");

        lblRealTime = new Label();
        lblRealTime.classList.AddRange(["game-text-normal", "text--yellow"]);
        icon.parent.Add(lblRealTime);
        lblRealTime.ToggleDisplayStyle(false);

        s.AddRealTimeClock.ValueChanged += (_, _ ) => OnSettingsChanged();
        s.Clock24.ValueChanged += (_, _ ) => OnSettingsChanged();
        OnSettingsChanged();
    }

    void OnSettingsChanged()
    {
        enabled = s.AddRealTimeClock.Value;
        clock24 = s.Clock24.Value;
    }

    public void UpdateSingleton()
    {
        if (enabled)
        {
            lastEnabled = true;

            var format = clock24 ? "HH:mm" : "hh:mmtt";
            lblRealTime.text = DateTime.Now.ToString(format);

            lblRealTime.ToggleDisplayStyle(true);
            icon.ToggleDisplayStyle(false);
        }
        else if (lastEnabled)
        {
            lastEnabled = false;
            lblRealTime.ToggleDisplayStyle(false);
            icon.ToggleDisplayStyle(true);
        }
    }

}
