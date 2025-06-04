namespace ModdableWeather.UI;

public class ModdableDatePanel(
    UILayout uiLayout,
    VisualElementLoader visualElementLoader,
    WeatherService weatherService,
    TimestampFormatter timestampFormatter,
    ILoc loc,
    ITooltipRegistrar tooltipRegistrar,
    EventBus eventBus,
    HazardousWeatherUIHelper hazardousWeatherUIHelper,
    GameCycleService gameCycleService
) : DatePanel(uiLayout, visualElementLoader, weatherService, timestampFormatter, loc, tooltipRegistrar, eventBus, hazardousWeatherUIHelper, gameCycleService),
    ILoadableSingleton, IUnloadableSingleton
{
    static ModdableDatePanel? instance;
    public static ModdableDatePanel Instance => instance.InstanceOrThrow();

    readonly ModdableWeatherService moddableWeatherService = (ModdableWeatherService)weatherService;

    VisualElement icon = null!;

    public new void Load()
    {
        instance = this;
        base.Load();
    }

    public void Unload()
    {
        instance = null;
    }

    public void NewUpdatePanel()
    {
        icon ??= _root.Q(className: "date-panel__icon")
            ?? throw new KeyNotFoundException("Cannot find DatePanel icon");

        var spec = moddableWeatherService.CurrentWeather.Spec;

        NewUpdateIcon(spec);
        NewUpdateText(spec);
    }

    void NewUpdateIcon(ModdedWeatherSpec spec)
    {
        icon.style.backgroundImage = new(spec.DatePanelIcon);
    }

    internal void NewUpdateText(ModdedWeatherSpec spec)
    {
        _text.text = _timestampFormatter.FormatLongLocalized(_gameCycleService.Cycle, _gameCycleService.CycleDay);
        _tooltipText = spec.Display;
    }

}
