namespace MoreHttpApi.Shared.Specs;

[GeneratedCode("MoreHttpApi", "10.0.0")]
[ComponentSpec("Timberborn.SkySystem.FogSettingsSpec")]
public record ParsedFogSettingsSpec(
    HttpSerializableFloats FogColor,
    Single FogDensity
) : ParsedComponentSpec;