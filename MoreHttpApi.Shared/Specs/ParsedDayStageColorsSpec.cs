namespace MoreHttpApi.Shared.Specs;

[GeneratedCode("MoreHttpApi", "10.0.0")]
[ComponentSpec("Timberborn.SkySystem.DayStageColorsSpec")]
public record ParsedDayStageColorsSpec(
    HttpSerializableFloats SunColor,
    Single SunIntensity,
    Single SunXAngle,
    Single ShadowStrength,
    HttpSerializableFloats AmbientSkyColor,
    HttpSerializableFloats AmbientEquatorColor,
    HttpSerializableFloats AmbientGroundColor,
    Single ReflectionsIntensity,
    ParsedFogSettingsSpec TemperateWeatherFog,
    ParsedHazardousWeatherFogSettingsSpec[] HazardousWeatherFogs
) : ParsedComponentSpec;