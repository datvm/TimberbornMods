namespace ModdableWeather.UI;

public class ModdableHazardousWeatherNotificationPanel(
    ILoc t,
    EventBus eventBus,
    HazardousWeatherUIHelper hazardousWeatherUIHelper,
    UILayout uiLayout,
    PanelStack panelStack,
    VisualElementLoader visualElementLoader,
    WeatherService weatherService,
    CameraHorizontalShifter cameraHorizontalShifter,
    ModdableWeatherHistoryProvider history,
    ModdableWeatherService moddableWeatherService
) : HazardousWeatherNotificationPanel(t, eventBus, hazardousWeatherUIHelper, uiLayout, panelStack, visualElementLoader, weatherService, cameraHorizontalShifter),
    ILoadableSingleton, IUnloadableSingleton
{
    readonly ILoc t = t;

    static ModdableHazardousWeatherNotificationPanel? instance;
    public static ModdableHazardousWeatherNotificationPanel Instance => instance.InstanceOrThrow();

    public new void Load()
    {
        instance = this;
        base.Load();
    }

    public void Unload()
    {
        instance = null;
    }

    [OnEvent]
    public void OnCycleWeatherDecided(OnModdableWeatherCycleDecided _)
    {
        ShowWeatherNotification(history.CurrentTemperateWeather, false, true);
    }

    public void NewShowHazardousSeasonNotification()
    {
        ShowWeatherNotification(
            history.CurrentHazardousWeather,
            !moddableWeatherService.IsHazardousWeather,
            false);
    }

    void ShowWeatherNotification(IModdableWeather weather, bool approaching, bool newCycle)
    {
        var spec = weather.Spec;

        _header.text = approaching ?
            spec.MessageApproaching :
            spec.MessageStart;

        if (newCycle)
        {
            _description.text = t.T(CycleBeginsKey, history.CurrentCycle.Cycle);
            _description.ToggleDisplayStyle(true);
        }
        else
        {
            _description.ToggleDisplayStyle(false);
        }

        _background.style.backgroundImage = new(spec.WeatherNotification);

        _panel.ToggleDisplayStyle(true);
        _notificationTimer = 0f;
        SetPanelFade(true);
    }

}