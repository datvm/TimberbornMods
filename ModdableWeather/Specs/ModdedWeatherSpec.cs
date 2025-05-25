namespace ModdableWeather.Specs;

public record ModdedWeatherSpec : ComponentSpec, IHazardousWeatherUISpecification
{

#nullable disable
    [Serialize]
    public string Id { get; init; } = null!;

    [Serialize]
    public string DisplayLoc { get; init; } = null!;
    public string Display { get; set; } = null!;
    public string MessageStart { get; set; } = null!;
    public string MessageEnd { get; set; } = null!;
    public string MessageApproaching { get; set; } = null!;
    public string MessageInProgress { get; set; } = null!;

    [Serialize]
    public Sprite WeatherPanelProgressBackground { get; init; } = null!;    
    [Serialize]
    public Sprite DatePanelIcon { get; init; } = null!;
    [Serialize]
    public Sprite WeatherNotification { get; init; } = null!;

    public ModdedWeatherSkySpec Sky { get; set; }
#nullable enable

    [Serialize]
    public Sprite? WeatherPanelBlinkBackground { get; init; }

    public string NameLocKey => DisplayLoc;
    public string ApproachingLocKey => MessageApproaching;
    public string InProgressLocKey => MessageInProgress;
    public string StartedNotificationLocKey => MessageStart;
    public string EndedNotificationLocKey => MessageEnd;
    public string InProgressClass => "";
    public string IconClass => "";
    public string NotificationBackgroundClass => "";
}
