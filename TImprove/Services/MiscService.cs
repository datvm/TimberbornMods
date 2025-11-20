namespace TImprove.Services;

public class MiscService(
    Sun sun,
    CameraService cam,
    IDayNightCycle time,
    MSettings s,
    DatePanel datePanel
) : ILoadableSingleton, ITickableSingleton
{

    Label lblTime = null!;
    bool gameTimeEnabled;

    public string GetGameTimeFormatted()
    {
        var hour = time.HoursPassedToday;
        var hourDivision = hour - MathF.Floor(hour);

        return $"{(int)hour:D2}:{(int)(hourDivision * 60):D2}";
    }

    public void Load()
    {
        var panel = datePanel._text.parent;

        lblTime = new Label();
        lblTime.classList.AddRange(["game-text-normal", "text--yellow"]);
        lblTime.style.flexShrink = 0;
        panel.Add(lblTime);

        s.ModSettingChanged += (_, _) => ApplyValues();
        ApplyValues();
    }

    void ApplyValues()
    {
        sun.Fog = !s.DisableFog.Value;
        cam.FreeMode = s.EnableFreeCamera.Value;
        
        gameTimeEnabled = s.ShowGameTime.Value;
        lblTime.ToggleDisplayStyle(gameTimeEnabled);
        Tick();
    }

    public void Tick()
    {
        if (gameTimeEnabled)
        {
            UpdateTimeClock();
        }
    }

    void UpdateTimeClock()
    {
        if (!s.ShowGameTime.Value) { return; }
        lblTime.text = GetGameTimeFormatted();
    }

}
