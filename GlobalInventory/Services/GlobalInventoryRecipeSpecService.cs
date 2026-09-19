namespace GlobalInventory.Services;

[BindSingleton]
public class GlobalInventoryRecipeSpecService(RecipeSpecService recipes, GlobalGoodSpecService goods) : ILoadableSingleton
{
    FrozenDictionary<string, GlobalInventoryRecipeSpec> byRecipeId =
        FrozenDictionary<string, GlobalInventoryRecipeSpec>.Empty;

    public void Load()
    {
        Dictionary<string, GlobalInventoryRecipeSpec> map = [];
        foreach (var recipe in recipes.GetRecipes())
        {
            var spec = recipe.GetSpec<GlobalInventoryRecipeSpec>();
            if (spec is null)
            {
                continue;
            }

            Validate(recipe, spec);
            map[recipe.Id] = spec;
        }

        byRecipeId = map.ToFrozenDictionary();
    }

    public bool TryGet(RecipeSpec? recipe, [NotNullWhen(true)] out GlobalInventoryRecipeSpec? spec)
    {
        spec = null;
        return recipe is not null && byRecipeId.TryGetValue(recipe.Id, out spec);
    }

    void Validate(RecipeSpec recipe, GlobalInventoryRecipeSpec spec)
    {
        foreach (var entry in spec.Ingredients.Concat(spec.Products))
        {
            if (string.IsNullOrEmpty(entry.Id) || entry.Amount <= 0)
            {
                throw new InvalidOperationException(
                    $"Recipe '{recipe.Id}' has an invalid global good amount '{entry.Id}' x {entry.Amount}.");
            }

            if (!goods.TryGet(entry.Id, out _))
            {
                throw new InvalidOperationException(
                    $"Recipe '{recipe.Id}' references unknown global good '{entry.Id}'.");
            }
        }
    }
}
