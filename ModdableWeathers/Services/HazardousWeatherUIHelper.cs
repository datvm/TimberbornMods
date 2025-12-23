namespace ModdableWeathers.Services;

[ReplaceSingleton]
[ThrowProperties([
    nameof(ApproachingLocKey),
    nameof(EndedNotificationLocKey),
    nameof(IconClass),
    nameof(InProgressClass),
    nameof(InProgressLocKey),
    nameof(NameLocKey),
    nameof(NotificationBackgroundClass),
    nameof(StartedNotificationLocKey),
])]
[ThrowMethods([
    nameof(OnHazardousWeatherSelected),
])]
[BypassMethods([
    nameof(Load),    
])]
public class ModdableHazardousWeatherUIHelper(HazardousWeatherService hazardousWeatherService, EventBus eventBus, DroughtWeatherUISpecification droughtWeatherUISpecification, BadtideWeatherUISpecification badtideWeatherUISpecification) : HazardousWeatherUIHelper(hazardousWeatherService, eventBus, droughtWeatherUISpecification, badtideWeatherUISpecification)
{
}
