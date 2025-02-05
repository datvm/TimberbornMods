
namespace TImprove.Services;

public class TImproveGameService(
    Sun sun,
    CameraComponent cam,
    IDayNightCycle time,
    MSettings s,
    IEnumerable<IDevModule> devMods,
    DatePanel datePanel,
    EventBus eventBus
) : ILoadableSingleton, IPostLoadableSingleton, IUnloadableSingleton, ITickableSingleton
{

    #region Game Time

    static readonly MethodInfo DatePanelUpdateText = typeof(DatePanel).GetMethod(nameof(DatePanel.UpdateText), BindingFlags.Instance | BindingFlags.NonPublic);
    static readonly FieldInfo DatePanelText = typeof(DatePanel).GetField("_text", BindingFlags.Instance | BindingFlags.NonPublic);

    #endregion

    static readonly MethodInfo SetSpeed = typeof(SpeedControlPanel).GetMethod(nameof(SetSpeed), BindingFlags.NonPublic | BindingFlags.Instance);
    readonly SpeedControlPanel speedPanel = (SpeedControlPanel)devMods
        .First(q => q is SpeedControlPanel);

    public string GetGameTimeFormatted()
    {
        var hour = time.HoursPassedToday;
        var hourDivision = hour - MathF.Floor(hour);

        return $"{(int)hour:D2}:{(int)(hourDivision * 60):D2}";
    }

    public void Load()
    {
        ApplyValues();
        s.OnSettingsChanged += ApplyValues;
        UpdateTimeClock();
    }

    void ApplyValues()
    {
        sun.Fog = !s.DisableFog;
        cam.FreeMode = s.EnableFreeCamera;

        sun.Update();
    }

    [OnEvent]
    public void OnHazardWeatherStarted(HazardousWeatherStartedEvent _)
    {
        if (!s.PauseBadWeather) { return; }

        PerformSetSpeed(0);
    }

    void PerformSetSpeed(float speed)
    {
        SetSpeed.Invoke(speedPanel, [speed]);
    }

    public void Tick()
    {
        UpdateTimeClock();
    }

    void UpdateTimeClock()
    {
        if (!s.ShowGameTime) { return; }

        DatePanelUpdateText.Invoke(datePanel, []);

        if (DatePanelText.GetValue(datePanel) is Label label)
        {
            label.text += ", " + GetGameTimeFormatted();
        }
    }

    public void Unload()
    {
        eventBus.Unregister(this);
    }

    public void PostLoad()
    {
        eventBus.Register(this);
    }
}
