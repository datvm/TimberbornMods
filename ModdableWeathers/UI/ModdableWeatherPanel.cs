namespace ModdableWeathers.UI;

[ReplaceSingleton]
[BypassMethods([
    nameof(OnHazardousWeatherApproaching)
])]
[ThrowMethods([
    nameof(UpdateHazardousWeatherClasses),
    nameof(SetApproachingHazardUI),
])]
public class ModdableWeatherPanel(
    WeatherCycleService weatherCycleService,
    ModdableWeatherApproachingTimer timer,

    UILayout uiLayout, EventBus eventBus, VisualElementLoader visualElementLoader, WeatherService weatherService, ILoc loc, GameUISoundController gameUISoundController, ITooltipRegistrar tooltipRegistrar, HazardousWeatherUIHelper hazardousWeatherUIHelper, GameCycleService gameCycleService, ISpecService specService)
    : WeatherPanel(uiLayout, eventBus, visualElementLoader, weatherService, loc, gameUISoundController, tooltipRegistrar, hazardousWeatherUIHelper, timer, gameCycleService, specService)
    , ILoadableSingleton
{

    readonly ModdableWeatherApproachingTimer timer = timer;
    readonly ILoc t = loc;

    IModdableWeather? currWeather, nextWeather;
    VisualElement currEl = null!, nextEl = null!;

    void ILoadableSingleton.Load()
    {
        Load(); // Call base load

        currEl = _root.Q(className: "weather-panel__progress-background");

        nextEl = _root.AddChild(classes: ["weather-panel__progress-progress"])
            .SetHeightPercent(100);
        var s = nextEl.style;
        s.backgroundSize = new BackgroundSize(BackgroundSizeType.Cover);
        s.flexDirection = FlexDirection.Row;

        nextEl.InsertSelfAfter(_simpleProgressBar);
        _simpleProgressBar.RemoveFromHierarchy();
    }

    [OnEvent]
    public void OnModdableWeatherApproaching(ModdableWeatherApproachingEvent e)
    {
        _startBlinkingIfUnpaused = true;
    }

    [ReplaceMethod]
    public void MUpdatePanel()
    {
        var curr = weatherCycleService.CurrentStage;
        var next = weatherCycleService.NextStage;

        UpdateBackground(curr.Weather, next.Weather);
        SetProgressUI(curr, next);
    }

    void UpdateBackground(IModdableWeather curr, IModdableWeather next)
    {
        if (curr == currWeather && next == nextWeather) { return; }

        currWeather = curr;
        nextWeather = next;

        currEl.style.backgroundImage = new(currWeather.Spec.WeatherPanelProgressBackground.Asset);
        nextEl.style.backgroundImage = new(nextWeather.Spec.WeatherPanelProgressBackground.Asset);
    }

    void SetProgressUI(DetailedWeatherStageReference curr, DetailedWeatherStageReference next)
    {
        var progress = timer.GetNextWeatherWarningProgress();

        var msg = new StringBuilder();
        msg.AppendLine(curr.Weather.Spec.MessageInProgress);

        if (progress >= 0f)
        {
            msg.AppendLine(next.Weather.Spec.MessageApproaching);
        }

        if (curr.WeatherModifiers.Length > 1)
        {
            msg.AppendLine(curr.ListEffects(t));
        }
        var msgText = msg.ToStringWithoutNewLineEndAndClean();

        if (progress < 0f)
        {
            SetPanelContent(msgText, 0f, 0f);
            SetProgressBar(0);
        }
        else
        {
            var blink = _remainingBlinks > 0 && NextBlinkingBarState();

            SetPanelContent(
                msgText,
                progress,
                weatherCycleService.PartialDaysUntilNextStage,
                blink
            );
            SetProgressBar(progress);
        }

        
    }

    void SetProgressBar(float progress)
    {
        nextEl.style.width = new Length(progress * 100f, LengthUnit.Percent);
    }

}
