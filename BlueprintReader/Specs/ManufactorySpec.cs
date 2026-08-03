namespace BlueprintReader.Specs;

public record ManufactorySpec(ImmutableArray<string> ProductionRecipeIds) : IBlueprintSpec;
