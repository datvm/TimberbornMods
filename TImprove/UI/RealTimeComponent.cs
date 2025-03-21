global using Timberborn.WorkSystemUI;

namespace TImprove.UI;

public class RealTimeComponent(WorkingHoursPanel workingHours) : IUpdatableSingleton, ILoadableSingleton
{
    VisualElement icon = null!;
    Label lblRealTime = null!;

    bool lastEnabled;

    public void Load()
    {
        icon = workingHours._root.Q(classes: "working-hours-panel__icon");

        lblRealTime = new Label();
        lblRealTime.classList.AddRange(["game-text-normal", "text--yellow"]);
        icon.parent.Add(lblRealTime);
        lblRealTime.ToggleDisplayStyle(false);
    }

    public void UpdateSingleton()
    {
        if (MSettings.Instance?.AddRealTimeClock == true)
        {
            lastEnabled = true;

            lblRealTime.text = DateTime.Now.ToString("hh:mmtt");

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
