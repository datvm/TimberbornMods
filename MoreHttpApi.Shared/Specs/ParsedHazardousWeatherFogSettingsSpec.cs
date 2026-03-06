namespace MoreHttpApi.Shared.Specs;

[GeneratedCode("MoreHttpApi", "10.0.0")]
[ComponentSpec("Timberborn.SkySystem.HazardousWeatherFogSettingsSpec")]
public record ParsedHazardousWeatherFogSettingsSpec(
    string HazardousWeatherId,
    ParsedFogSettingsSpec FogSettings
) : ParsedComponentSpec;