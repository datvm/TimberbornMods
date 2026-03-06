namespace MoreHttpApi.Shared.Specs;

[GeneratedCode("MoreHttpApi", "10.0.0")]
[ComponentSpec("Timberborn.SkySystem.SunSpec")]
public record ParsedSunSpec(
    string SunPrefab,
    ParsedDayStageColorsSpec SunriseColors,
    ParsedDayStageColorsSpec DayColors,
    ParsedDayStageColorsSpec SunsetColors,
    ParsedDayStageColorsSpec NightColors,
    Single RotateWithCameraOffset
) : ParsedComponentSpec;