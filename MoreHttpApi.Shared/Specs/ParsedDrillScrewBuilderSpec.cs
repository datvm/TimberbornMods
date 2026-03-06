namespace MoreHttpApi.Shared.Specs;

[GeneratedCode("MoreHttpApi", "10.0.0")]
[ComponentSpec("Timberborn.Terraforming.DrillScrewBuilderSpec")]
public record ParsedDrillScrewBuilderSpec(
    string ParentName,
    HttpSerializableFloats AnchorPosition,
    Single DrillRadius,
    string ScrewHeadPrefabPath,
    string ScrewAxisPrefabPath
) : ParsedComponentSpec;