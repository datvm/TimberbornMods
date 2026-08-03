namespace BlueprintReader.Specs;

public record RecipeSpec(
    string Id,
    ImmutableArray<GoodAmountSpec> Ingredients,
    ImmutableArray<GoodAmountSpec> Products,
    string? Fuel
) : IIdBlueprintSpec;
