global using Timberborn.TimeSystemUI;

namespace LunchBreak.UI;

public class LunchBreakClockPanel(ClockPanel clockPanel, LunchBreakManager man) : ILoadableSingleton, IUpdatableSingleton
{
    readonly RotatingBackground[] needles = new RotatingBackground[2];

    public void Load()
    {
        var root = clockPanel._root;

        for (int i = 0; i < needles.Length; i++)
        {
            var needle = needles[i] = new()
            {
                name = i == 0 ? "StartLunchBreak" : "EndLunchBreak",
            };
            needle.classList.Add("clock-panel__item");
            needle.classList.Add("clock-panel__working-hours-needle");

            root.Add(needle);
        }
    }

    public void UpdateSingleton()
    {
        UpdateMovingParts();
    }

    void UpdateMovingParts()
    {
        var hours = man.LunchBreakTime;

        for (int i = 0; i < 2; i++)
        {
            needles[i].SetRotation(ClockPanel.NormalizeRotation(hours[i] / 24f));
        }
    }

}
