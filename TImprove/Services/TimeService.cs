namespace TImprove.Services;

public class TimeService(
    Sun sun,
    CameraService cam,
    IDayNightCycle time,
    MSettings s,
    DatePanel datePanel
) : ILoadableSingleton, ITickableSingleton
{

    Label lblTime = null!;

    public string GetGameTimeFormatted()
    {
        var hour = time.HoursPassedToday;
        var hourDivision = hour - MathF.Floor(hour);

        return $"{(int)hour:D2}:{(int)(hourDivision * 60):D2}";
    }

    public void Load()
    {
        lblTime = datePanel._text;

        s.ModSettingChanged += (_, _) => ApplyValues();
        ApplyValues();
        UpdateTimeClock();
    }

    void ApplyValues()
    {
        sun.Fog = !s.DisableFog;
        cam.FreeMode = s.EnableFreeCamera;

        sun.LateUpdateSingleton();
    }

    public void Tick()
    {
        UpdateTimeClock();
    }

    void UpdateTimeClock()
    {
        if (!s.ShowGameTime) { return; }

        var text = lblTime.text;
        var index = text.IndexOf('|');
        if (index > -1)
        {
            text = text[..index].TrimEnd();
        }
        text += " | " + GetGameTimeFormatted();

        lblTime.text = text;
    }

}
