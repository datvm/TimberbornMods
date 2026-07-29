namespace TechTreeBlueprintScript.Specs;

public record BuildingSpec(
    ImmutableArray<GoodAmountSpec> BuildingCost,
    int ScienceCost
) : IBlueprintSpec;