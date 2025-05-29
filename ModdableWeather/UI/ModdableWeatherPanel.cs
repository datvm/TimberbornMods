namespace ModdableWeather.UI;

public class ModdableWeatherPanel(
    UILayout uiLayout,
    EventBus eventBus,
    VisualElementLoader visualElementLoader,
    WeatherService weatherService,
    ILoc loc,
    GameUISoundController gameUISoundController,
    ITooltipRegistrar tooltipRegistrar,
    HazardousWeatherUIHelper hazardousWeatherUIHelper,
    HazardousWeatherApproachingTimer hazardousWeatherApproachingTimer,
    GameCycleService gameCycleService,
    ModdableWeatherHistoryProvider history
) : WeatherPanel(uiLayout, eventBus, visualElementLoader, weatherService, loc, gameUISoundController, tooltipRegistrar, hazardousWeatherUIHelper, hazardousWeatherApproachingTimer, gameCycleService),
    ILoadableSingleton, IUnloadableSingleton
{
    static ModdableWeatherPanel? instance;
    public static ModdableWeatherPanel Instance => instance.InstanceOrThrow();

    VisualElement backgroundEl = null!;
    VisualElement progressEl = null!;

    IModdedWeather? bgWeather, progressBg;
    readonly ModdableWeatherService weatherService = (ModdableWeatherService)weatherService;

    bool isHazardousWeather;
    float hazardApproachingProgress, partialCycleDay;
    int hazardousWeatherStartCycleDay;
    bool progressReverse;
    IModdedHazardousWeather currHazardousWeather = null!;

    public new void Load()
    {
        instance = this;
        base.Load();

        backgroundEl = _root.Q(className: "weather-panel__progress-background");

        progressEl = _root.AddChild(classes: ["weather-panel__progress-progress"])
            .SetHeightPercent(100);
        progressEl.style.backgroundSize = new BackgroundSize(BackgroundSizeType.Cover);
        progressEl.style.flexDirection = FlexDirection.Row;

        progressEl.InsertSelfAfter(_simpleProgressBar);
        _simpleProgressBar.RemoveFromHierarchy();
    }

    public void Unload()
    {
        instance = null;
    }

    public new void UpdatePanel()
    {
        isHazardousWeather = weatherService.IsHazardousWeather;
        currHazardousWeather = weatherService.WeatherCycleDetails.HazardousWeather;
        hazardApproachingProgress = _hazardousWeatherApproachingTimer.GetProgress();
        hazardousWeatherStartCycleDay = weatherService.HazardousWeatherStartCycleDay;
        partialCycleDay = _gameCycleService.PartialCycleDay;

        UpdateHazardousWeatherClasses();
        var reverse = false;

        if (isHazardousWeather)
        {
            SetHazardousWeatherUI();
        }
        else if (hazardApproachingProgress > 0f)
        {
            reverse = true;
            SetApproachingHazardUI();
        }
        else
        {
            SetPanelContent(weatherService.WeatherCycleDetails.TemperateWeather.Spec.MessageInProgress, 0f, 0f);
            SetProgressBar(0f);
        }

        if (progressReverse != reverse)
        {
            progressReverse = reverse;
            backgroundEl.style.flexDirection = reverse ? FlexDirection.RowReverse : FlexDirection.Row;
        }
    }

    new void UpdateHazardousWeatherClasses()
    {
        var bg = isHazardousWeather ? history.NextCycleTemperateWeather : history.CurrentTemperateWeather;
        var progress = currHazardousWeather;

        if (bg == bgWeather && progress == progressBg) { return; }

        bgWeather = bg;
        progressBg = progress;

        backgroundEl.style.backgroundImage = new(bg.Spec.WeatherPanelProgressBackground);
        progressEl.style.backgroundImage = new(progress.Spec.WeatherPanelProgressBackground);
    }

    void SetHazardousWeatherUI()
    {
        var duration = _weatherService.HazardousWeatherDuration;
        var passedDays = partialCycleDay - hazardousWeatherStartCycleDay;

        var forecastCount = duration - passedDays;
        var progressBarValue = 1 - passedDays / duration;
        var inProgressLocKey = currHazardousWeather.Spec.MessageInProgress;

        SetPanelContent(inProgressLocKey, progressBarValue, forecastCount);
        SetProgressBar(progressBarValue);
    }

    void SetApproachingHazardUI()
    {
        var blink = _remainingBlinks > 0 && NextBlinkingBarState();

        SetPanelContent(
            currHazardousWeather.Spec.MessageApproaching,
            hazardApproachingProgress,
            hazardousWeatherStartCycleDay - partialCycleDay,
            blink);
        SetProgressBar(hazardApproachingProgress);
    }

    public void SetProgressBar(float progress)
    {
        progressEl.style.width = new Length(progress * 100f, LengthUnit.Percent);
    }

}
