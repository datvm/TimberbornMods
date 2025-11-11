namespace ConfigurablePumps.Services;

public class WaterRecipeModifier : BaseSpecTransformer<RecipeSpec>, ISpecModifier
{
    public const string WaterId = "Water";
    public const string BadwaterId = "Badwater";

    bool ISpecModifier.ShouldRun => MSettings.WaterProdTimeMultiplier != 1f;

    public override RecipeSpec? Transform(RecipeSpec spec)
    {
        if (!spec.Products.Any(p => IsPumpProduct(p.Id))) { return null; }

        return spec with
        {
            CycleDurationInHours = spec.CycleDurationInHours * MSettings.WaterProdTimeMultiplier,
        };
    }

    static bool IsPumpProduct(string id) => id == WaterId || id == BadwaterId;

}
