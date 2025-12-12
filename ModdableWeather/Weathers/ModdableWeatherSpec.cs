namespace ModdableWeather.Weathers;

public record ModdableWeatherSpec : ComponentSpec, IHazardousWeatherUISpecification
{

#nullable disable
    [Serialize]
    public string Id { get; init; }

    [Serialize]
    public int Order { get; init; }

    [Serialize]
    public string DisplayLoc { get; init; }

    [Serialize(nameof(DisplayLoc))]
    public LocalizedText Display { get; set; }

    public string MessageStart { get; set; }
    public string MessageEnd { get; set; }
    public string MessageApproaching { get; set; }
    public string MessageInProgress { get; set; }

    [Serialize]
    public AssetRef<Sprite> WeatherPanelProgressBackground { get; init; }
    [Serialize]
    public AssetRef<Sprite> DatePanelIcon { get; init; }
    [Serialize]
    public AssetRef<Sprite> WeatherNotification { get; init; }
#nullable enable

    [Serialize]
    public AssetRef<Sprite>? WeatherPanelBlinkBackground { get; init; }

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
