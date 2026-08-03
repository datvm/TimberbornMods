namespace BlueprintReader.Specs;

public record BuildingSpec(
    ImmutableArray<GoodAmountSpec> BuildingCost,
    int ScienceCost
) : IBlueprintSpec;
