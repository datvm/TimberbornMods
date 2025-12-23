namespace ModdableWeathers.UI;

[ReplaceSingleton]
[BypassMethods([
    nameof(OnCycleEndedEvent),
    nameof(OnHazardousWeatherStarted),
    nameof(OnHazardousWeatherApproaching),
])]
[ThrowMethods([
    nameof(ShowNotification),
])]
public class ModdableHazardousWeatherNotificationPanel(
    ILoc loc, EventBus eventBus, HazardousWeatherUIHelper hazardousWeatherUIHelper, UILayout uiLayout, PanelStack panelStack, VisualElementLoader visualElementLoader, WeatherService weatherService, CameraHorizontalShifter cameraHorizontalShifter, ISpecService specService)
    : HazardousWeatherNotificationPanel(loc, eventBus, hazardousWeatherUIHelper, uiLayout, panelStack, visualElementLoader, weatherService, cameraHorizontalShifter, specService)
    , IUpdatableSingleton
{

    DetailedWeatherStageReference? approachingWeather;

    [OnEvent]
    public void OnWeatherTransitioned(WeatherTransitionedEvent e)
    {
        approachingWeather = null; // Clear any pending approaching weather
        var (_, to) = e;

        if (to.Cycle.Cycle == 1 && to.Stage.Index == 0) { return; }
        ShowWeatherNotification(to, false);
    }

    [OnEvent]
    public void OnNextWeatherStageApproaching(ModdableWeatherApproachingEvent e)
    {
        approachingWeather = e.NextStage;
    }

    void IUpdatableSingleton.UpdateSingleton()
    {
        if (_panel.IsDisplayed())
        {
            UpdateTimer();
            UpdatePanelPosition();
        }

        if (approachingWeather.HasValue && Time.timeScale != 0f)
        {
            ShowApproachingNotification();
        }
    }

    void ShowApproachingNotification()
    {
        var w = approachingWeather;
        if (w is null) { return; }

        approachingWeather = null;
        ShowWeatherNotification(w.Value, true);
    }

    void ShowWeatherNotification(DetailedWeatherStageReference stage, bool approaching)
    {
        var weatherSpec = stage.Weather.Spec;
        _header.text = approaching ? weatherSpec.MessageApproaching : weatherSpec.MessageStart;

        var description = new StringBuilder(Environment.NewLine);
        if (!approaching)
        {
            description.AppendLine(_loc.T("LV.MW.NewStageNotf", stage.Cycle.Cycle, stage.Stage.Index + 1));
        }

        var mods = stage.WeatherModifiers;
        if (mods.Length > 1)
        {
            description.Append(stage.ListEffects(_loc));
        }
        _description.text = description.ToStringWithoutNewLineEndAndClean();
        _description.ToggleDisplayStyle(!string.IsNullOrEmpty(_description.text));

        _background.style.backgroundImage = new(weatherSpec.WeatherNotification.Asset);

        _panel.ToggleDisplayStyle(true);
        _notificationTimer = 0f;
        SetPanelFade(true);
    }

}
