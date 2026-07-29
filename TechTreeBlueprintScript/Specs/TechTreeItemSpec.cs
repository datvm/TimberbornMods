namespace TechTreeBlueprintScript.Specs;

public record TechTreeItemSpec(
    ImmutableArray<string> Tags,
    ImmutableArray<string> Prerequisites
);