namespace GlobalInventory.Specs;

public readonly record struct GlobalGoodAmountSpec
{
    [Serialize]
    public string Id { get; init; }

    [Serialize]
    public float Amount { get; init; }
}

public record GlobalInventoryRecipeSpec : ComponentSpec
{
    [Serialize]
    public ImmutableArray<GlobalGoodAmountSpec> GlobalIngredients { get; init; } = [];

    [Serialize]
    public ImmutableArray<GlobalGoodAmountSpec> GlobalProducts { get; init; } = [];

    [Serialize]
    public bool AllowOverflow { get; init; }

    public ImmutableArray<GlobalGoodAmountSpec> Ingredients => GlobalIngredients.IsDefault ? [] : GlobalIngredients;
    public ImmutableArray<GlobalGoodAmountSpec> Products => GlobalProducts.IsDefault ? [] : GlobalProducts;
}

public record GlobalInventoryManufactorySpec : ComponentSpec;
