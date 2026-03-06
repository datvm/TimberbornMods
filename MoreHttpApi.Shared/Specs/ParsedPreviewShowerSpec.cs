namespace MoreHttpApi.Shared.Specs;

[GeneratedCode("MoreHttpApi", "10.0.0")]
[ComponentSpec("Timberborn.BlockObjectTools.PreviewShowerSpec")]
public record ParsedPreviewShowerSpec(
    HttpSerializableFloats BuildablePreview,
    HttpSerializableFloats UnbuildablePreview,
    HttpSerializableFloats WarningPreview
) : ParsedComponentSpec;