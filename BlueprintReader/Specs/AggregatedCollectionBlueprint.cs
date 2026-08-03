namespace BlueprintReader.Specs;

public record AggregatedCollectionBlueprint(
    Type Type,
    FrozenDictionary<string, ImmutableArray<string>> Collections
);
