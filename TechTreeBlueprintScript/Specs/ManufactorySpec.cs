namespace TechTreeBlueprintScript.Specs;

public record ManufactorySpec(ImmutableArray<string> ProductionRecipeIds) : IBlueprintSpec;
