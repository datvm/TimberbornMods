namespace TechTreeBlueprintScript.Specs;

public record AggregatedCollectionBlueprint(
    Type Type,
    FrozenDictionary<string, ImmutableArray<string>> Collections
);