namespace MoreHttpApi.Shared.Specs;

[GeneratedCode("MoreHttpApi", "10.0.0")]
[ComponentSpec("Timberborn.ForestryUI.TreeCuttingColorsSpec")]
public record ParsedTreeCuttingColorsSpec(
    HttpSerializableFloats ToolActionTile,
    HttpSerializableFloats ToolNoActionTile,
    HttpSerializableFloats CuttingAreaTile,
    HttpSerializableFloats CuttingAreaHighlight
) : ParsedComponentSpec;