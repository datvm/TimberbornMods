namespace MoreHttpApi.Shared.Specs;

[GeneratedCode("MoreHttpApi", "10.0.0")]
[ComponentSpec("Timberborn.Terraforming.DrillSpec")]
public record ParsedDrillSpec(
    HttpSerializableInts[] DrillableCoordinates,
    string RemovalEffectPath
) : ParsedComponentSpec;