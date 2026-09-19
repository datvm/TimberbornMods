namespace GlobalInventory.Services;

[BindSingleton]
public class GlobalInventoryManufactoryService(
    GlobalInventoryService inventory,
    GlobalInventoryRecipeSpecService recipes,
    ProductionItemFactory productionItems,
    DescribedAmountFactory amounts
)
{
    public bool TryGetRecipe(RecipeSpec? recipe, [NotNullWhen(true)] out GlobalInventoryRecipeSpec? spec)
        => recipes.TryGet(recipe, out spec);

    public float ProductionEfficiency(RecipeSpec? recipe, bool ingredientsTaken)
    {
        if (!TryGetRecipe(recipe, out var spec))
        {
            return 1f;
        }

        if (!ingredientsTaken && !HasIngredients(spec))
        {
            return 0f;
        }

        if (!spec.AllowOverflow && !HasProductCapacity(spec))
        {
            return 0f;
        }

        return 1f;
    }

    public bool HasIngredients(GlobalInventoryRecipeSpec spec)
    {
        foreach (var entry in spec.Ingredients)
        {
            var handle = inventory.Get(entry.Id);
            if (handle.Amount - entry.Amount < handle.MinCapacity)
            {
                return false;
            }
        }

        return true;
    }

    public bool HasProductCapacity(GlobalInventoryRecipeSpec spec)
    {
        foreach (var entry in spec.Products)
        {
            var handle = inventory.Get(entry.Id);
            if (handle.MaxCapacity is float max && handle.Amount + entry.Amount > max)
            {
                return false;
            }
        }

        return true;
    }

    public void TakeIngredients(GlobalInventoryRecipeSpec spec)
    {
        foreach (var entry in spec.Ingredients)
        {
            inventory.Get(entry.Id).Remove(entry.Amount);
        }
    }

    public void GiveProducts(GlobalInventoryRecipeSpec spec)
    {
        foreach (var entry in spec.Products)
        {
            var handle = inventory.Get(entry.Id);
            handle.AddSafe(entry.Amount);
        }
    }

    public IEnumerable<EntityDescription> Describe(Manufactory manufactory)
    {
        for (var i = 0; i < manufactory.ProductionRecipes.Length; i++)
        {
            var recipe = manufactory.ProductionRecipes[i];
            if (!TryGetRecipe(recipe, out var spec))
            {
                continue;
            }

            var content = productionItems.CreateInputOutput(
                DescribeAmounts(spec.Ingredients),
                DescribeAmounts(spec.Products),
                "");
            yield return EntityDescription.CreateInputOutputSection(content, 100 + i);
        }
    }

    IEnumerable<VisualElement> DescribeAmounts(ImmutableArray<GlobalGoodAmountSpec> entries)
    {
        foreach (var entry in entries)
        {
            var spec = inventory.Get(entry.Id).Spec;
            var icon = spec.IconSmall?.Value ?? spec.Icon?.Asset;
            if (icon is not null)
            {
                yield return amounts.CreatePlain("", GlobalInventoryUi.Format(entry.Amount), icon, spec.DisplayName.Value);
            }
            else
            {
                yield return amounts.CreatePlain("", GlobalInventoryUi.Format(entry.Amount), spec.DisplayName.Value);
            }
        }
    }
}
