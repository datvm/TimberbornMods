namespace Omnibar.Services.Omnibar.Providers;

public class OmnibarRecipeProvider(
    OmnibarToolProvider omnibarToolProvider,
    ILoc t,
    ProductionItemFactory productionItemFactory,
    DescribedAmountFactory describedAmountFactory,
    GoodDescriber goodDescriber,
    RecipeSpecService recipes
) : IOmnibarProvider, ILoadableSingleton
{

    OmnibarRecipeItem[] items = [];

    public void Load()
    {
        items = [.. omnibarToolProvider.Items
            .SelectMany(GetOmnibarRecipeItem)];
    }

    IEnumerable<OmnibarRecipeItem> GetOmnibarRecipeItem(OmnibarToolItem toolItem)
    {
        if (!toolItem.CanAddToTodoList || !toolItem.BuildingSpec) { yield break; }

        var man = toolItem.BuildingSpec.GetComponentFast<ManufactorySpec>();
        if (!man) { yield break; }

        foreach (var recipeId in man.ProductionRecipeIds)
        {
            var recipe = recipes.GetRecipe(recipeId);

            yield return new(new(recipe, toolItem.BuildingSpec), toolItem,
                t, productionItemFactory, describedAmountFactory, goodDescriber);
        }
    }

    public IReadOnlyList<OmnibarFilteredItem> ProvideItems(string filter)
    {
        if (filter.IsCommand()) { return []; }

        return OmnibarUtils.StandardFilter(items, filter, q => q.Title);
    }

}

public class OmnibarRecipeItem(
    RecipeBuilding recipeBuilding,
    OmnibarToolItem toolItem,
    ILoc t,
    ProductionItemFactory productionItemFactory,
    DescribedAmountFactory describedAmountFactory,
    GoodDescriber goodDescriber
) : IOmnibarItemWithTodoList
{
    public string Title { get; } = recipeBuilding.Recipe.DisplayLocKey.T(t);
    public IOmnibarDescriptor? Description { get; } = new OmnibarRecipeDescriptor(recipeBuilding, toolItem,
        productionItemFactory, describedAmountFactory, goodDescriber, t);
    public bool CanAddToTodoList { get; } = true;

    public void AddToTodoList(bool append) => toolItem.AddToTodoList(append);

    public void Execute()
    {
        toolItem.Execute();
    }

    public bool SetIcon(Image image)
    {
        if (recipeBuilding.Recipe.Icon is null) { return false; }

        image.sprite = recipeBuilding.Recipe.Icon;
        return true;
    }
}

public readonly record struct RecipeBuilding(RecipeSpec Recipe, BuildingSpec Building);