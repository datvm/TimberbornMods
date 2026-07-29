namespace TechTreeBlueprintScript.Specs;

public record FactionSpec(
    string Id,
    ImmutableArray<string> TemplateCollectionIds
) : IIdBlueprintSpec;
