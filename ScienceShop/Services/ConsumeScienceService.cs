namespace ScienceShop.Services;

[BindSingleton]
public class ConsumeScienceService(
    ConsumeScienceRecipeSpecService recipes,
    RecipeSpecService recipeSpecs,
    ScienceService science,
    DescribedAmountFactory amounts,
    NamedIconProvider icons,
    ILoc t
) : ILoadableSingleton, IUnloadableSingleton
{
    public static ConsumeScienceService? Instance { get; private set; }

    public int AvailableScience => science.SciencePoints;

    public void Load() => Instance = this;

    public void Unload() => Instance = null;

    public bool TryGetRecipe(RecipeSpec? recipe, [NotNullWhen(true)] out ConsumeScienceRecipeSpec? spec)
        => recipes.TryGet(recipe, out spec);

    public int GetOrder(RecipeSpec recipe) => recipes.GetOrder(recipe);

    public RecipeSpec? FindRecipe(string? id)
    {
        if (string.IsNullOrEmpty(id))
        {
            return null;
        }

        foreach (var recipe in recipeSpecs.GetRecipes())
        {
            if (recipe.Id == id)
            {
                return recipe;
            }
        }

        return null;
    }

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

    public void AddToRecipeVisuals(RecipeSpec? recipe, VisualElement inputRoot)
    {
        if (!TryGetRecipe(recipe, out var spec))
        {
            return;
        }

        var input = amounts.CreatePlain(
            "described-amount--science",
            spec.ScienceCost.ToString(),
            icons.Science,
            t.T("Science.SciencePoints"));
        RecipeDescriptionUi.AddInputs(inputRoot, [input]);
    }
}
