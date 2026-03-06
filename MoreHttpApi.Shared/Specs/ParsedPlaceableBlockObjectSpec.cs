namespace MoreHttpApi.Shared.Specs;

[GeneratedCode("MoreHttpApi", "10.0.0")]
[ComponentSpec("Timberborn.BlockSystem.PlaceableBlockObjectSpec")]
public record ParsedPlaceableBlockObjectSpec(
    string ToolGroupId,
    Int32 ToolOrder,
    ParsedToolShapes ToolShape,
    ParsedBlockObjectLayout Layout,
    ParsedCustomPivotSpec CustomPivot,
    Boolean CanBeAttachedToTerrainSide,
    Boolean DevModeTool
) : ParsedComponentSpec;