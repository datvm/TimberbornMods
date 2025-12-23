namespace ModdableWeathers.UI;

[ReplaceSingleton]
public class ModdableDatePanel(
    WeatherCycleService weatherCycleService,
    UILayout uiLayout, VisualElementLoader visualElementLoader, WeatherService weatherService, TimestampFormatter timestampFormatter, ILoc loc, ITooltipRegistrar tooltipRegistrar, EventBus eventBus, HazardousWeatherUIHelper hazardousWeatherUIHelper, GameCycleService gameCycleService)
    : DatePanel(uiLayout, visualElementLoader, weatherService, timestampFormatter, loc, tooltipRegistrar, eventBus, hazardousWeatherUIHelper, gameCycleService)
{

    VisualElement? icon;

    [ReplaceMethod]
    public void MUpdatePanel()
    {
        var w = weatherCycleService.CurrentWeather;

        icon ??= _root.Q(className: "date-panel__icon")
            ?? throw new InvalidOperationException("Could not find date panel icon element.");
        icon.style.backgroundImage = new(w.Spec.DatePanelIcon.Asset);
        _text.text = _timestampFormatter.FormatLongLocalized(_gameCycleService.Cycle, _gameCycleService.CycleDay);
        _tooltipText = w.Spec.Display.Value;
    }

}
