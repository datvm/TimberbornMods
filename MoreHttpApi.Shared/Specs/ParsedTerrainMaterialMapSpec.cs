namespace MoreHttpApi.Shared.Specs;

[GeneratedCode("MoreHttpApi", "10.0.0")]
[ComponentSpec("Timberborn.TerrainSystemRendering.TerrainMaterialMapSpec")]
public record ParsedTerrainMaterialMapSpec(
    string DesertTexture,
    string DryFieldTexture,
    string WetFieldTexture,
    string BlendingNoise,
    Single BlendingNoiseScale,
    Single BlendingNoiseMultiplier,
    Single BlendingSoftness,
    Single BlendingMargin,
    Single AltitudeCeiling,
    string AltitudeMultiplier,
    string DesertAltitudeMultiplier,
    Single CutoutMargin
) : ParsedComponentSpec;