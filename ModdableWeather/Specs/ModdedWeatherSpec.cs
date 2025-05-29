namespace ModdableWeather.Specs;

public record ModdedWeatherSpec : ComponentSpec, IHazardousWeatherUISpecification
{

#nullable disable
    [Serialize]
    public string Id { get; init; }

    [Serialize]
    public string DisplayLoc { get; init; }
    public string Display { get; set; }
    public string MessageStart { get; set; }
    public string MessageEnd { get; set; }
    public string MessageApproaching { get; set; }
    public string MessageInProgress { get; set; }

    [Serialize]
    public Sprite WeatherPanelProgressBackground { get; init; }
    [Serialize]
    public Sprite DatePanelIcon { get; init; }
    [Serialize]
    public Sprite WeatherNotification { get; init; }

    public ModdedWeatherSkySpec Sky { get; set; }
#nullable enable

    [Serialize]
    public Sprite? WeatherPanelBlinkBackground { get; init; }

    [Serialize]
    public string? StartSound { get; set; }

    public string NameLocKey => DisplayLoc;
    public string ApproachingLocKey => MessageApproaching;
    public string InProgressLocKey => MessageInProgress;
    public string StartedNotificationLocKey => MessageStart;
    public string EndedNotificationLocKey => MessageEnd;
    public string InProgressClass => "";
    public string IconClass => "";
    public string NotificationBackgroundClass => "";
}
