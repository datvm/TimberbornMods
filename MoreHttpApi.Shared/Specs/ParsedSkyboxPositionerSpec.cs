namespace MoreHttpApi.Shared.Specs;

[GeneratedCode("MoreHttpApi", "10.0.0")]
[ComponentSpec("Timberborn.SkySystem.SkyboxPositionerSpec")]
public record ParsedSkyboxPositionerSpec(
    string Skybox,
    Single DayProgressSunrise,
    Single DayProgressDay,
    Single DayProgressSunset,
    Single DayProgressNight
) : ParsedComponentSpec;