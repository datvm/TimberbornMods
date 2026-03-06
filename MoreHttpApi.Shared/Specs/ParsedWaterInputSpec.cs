namespace MoreHttpApi.Shared.Specs;

[GeneratedCode("MoreHttpApi", "10.0.0")]
[ComponentSpec("Timberborn.WaterBuildings.WaterInputSpec")]
public record ParsedWaterInputSpec(
    HttpSerializableInts WaterInputCoordinates,
    Int32 MaxDepth,
    string PipeSegmentPrefabPath,
    string PipeParentName
) : ParsedComponentSpec;