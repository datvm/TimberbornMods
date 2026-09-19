namespace ScienceShop.Services;

[BindSingleton]
public class ConsumeScienceService(
    ConsumeScienceRecipeSpecService recipes,
    ScienceService science,
    ProductionItemFactory productionItems,
    DescribedAmountFactory amounts,
    NamedIconProvider icons
)
{
    public int AvailableScience => science.SciencePoints;

    public bool TryGetRecipe(RecipeSpec? recipe, [NotNullWhen(true)] out ConsumeScienceRecipeSpec? spec)
        => recipes.TryGet(recipe, out spec);

    public bool HasEnough(ConsumeScienceRecipeSpec spec) => science.SciencePoints >= spec.ScienceCost;

    public void TakeScience(ConsumeScienceRecipeSpec spec) => science.SubtractPoints(spec.ScienceCost);

    public float ProductionEfficiency(RecipeSpec? recipe, bool running, bool scienceTaken)
    {
        if (!TryGetRecipe(recipe, out var spec))
        {
            return 1f;
        }

        if (!running)
        {
            return 0f;
        }

        if (!scienceTaken && !HasEnough(spec))
        {
            return 0f;
        }

        return 1f;
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

            var input = amounts.CreatePlain("", spec.ScienceCost.ToString(), icons.Science, "Science");
            var content = productionItems.CreateInputOutput([input], [], "");
            yield return EntityDescription.CreateInputOutputSection(content, 200 + i);
        }
    }
}
