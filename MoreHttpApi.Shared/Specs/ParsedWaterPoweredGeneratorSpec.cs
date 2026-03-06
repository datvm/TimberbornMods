namespace MoreHttpApi.Shared.Specs;

[GeneratedCode("MoreHttpApi", "10.0.0")]
[ComponentSpec("Timberborn.PowerGeneration.WaterPoweredGeneratorSpec")]
public record ParsedWaterPoweredGeneratorSpec(
    HttpSerializableInts[] Blocks,
    HttpSerializableFloats ExpectedWaterDirection,
    Single MinRequiredOutflow
) : ParsedComponentSpec;