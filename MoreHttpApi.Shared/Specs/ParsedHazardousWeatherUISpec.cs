namespace MoreHttpApi.Shared.Specs;

[GeneratedCode("MoreHttpApi", "10.0.0")]
[ComponentSpec("Timberborn.HazardousWeatherSystemUI.HazardousWeatherUISpec")]
public record ParsedHazardousWeatherUISpec(
    Int32 ApproachingNotificationDays,
    Single MaxDayProgressLeftToNotify,
    Single NotificationDuration
) : ParsedComponentSpec;