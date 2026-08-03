namespace BlueprintReader.Specs;

public record PlaceableBlockObjectSpec(
    string ToolGroupId,
    int ToolOrder,
    bool DevModeTool = false
) : IBlueprintSpec;
