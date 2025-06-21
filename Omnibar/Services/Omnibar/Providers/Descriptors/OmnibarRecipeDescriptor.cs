namespace Omnibar.Services.Omnibar.Providers.Descriptors;

public class OmnibarRecipeDescriptor(
    RecipeBuilding item,
    OmnibarToolItem toolItem,
    ProductionItemFactory productionItemFactory,
    DescribedAmountFactory describedAmountFactory,
    GoodDescriber goodDescriber,
    ILoc t
) : IOmnibarDescriptor
{

    public bool Describe(VisualElement el)
    {
        DescribeRecipe(el);
        DescribeBuilding(el);

        return true;
    }

    void DescribeRecipe(VisualElement el)
    {
        var inputs = item.Recipe.Ingredients.Select(DescribeGood);
        var outputs = item.Recipe.Products.Select(DescribeGood);

        var recipeEl = productionItemFactory
            .CreateInputOutput(inputs, outputs, GetCraftingTime())
            .SetMarginRight();
        //recipeEl.Q("Input").style.width = recipeEl.Q("Output").style.width = new StyleLength(StyleKeyword.Auto);

        el.Add(recipeEl);
    }

    void DescribeBuilding(VisualElement el)
    {
        el.AddGameLabel(item.Building.GetComponentFast<LabeledEntitySpec>().DisplayNameLocKey.T(t).Bold())
            .SetMarginRight();
        toolItem.Description?.Describe(el);
    }

    VisualElement DescribeGood(GoodAmountSpecNew spec)
    {
        var describedGood = goodDescriber.GetDescribedGood(spec.Id);

        return describedAmountFactory.CreatePlain("",
            spec.Amount.ToString(),
            describedGood.Icon,
            describedGood.DisplayName);
    }

    /// <summary>
    /// From ManufactoryDescriber.GetCraftingTime
    /// </summary>
    /// <returns></returns>
    string GetCraftingTime()
    {
        var workplace = item.Building.GetComponentFast<WorkplaceSpec>();
        var workers = workplace ? workplace.MaxWorkers : 1;

        float num = item.Recipe.CycleDurationInHours / workers;
        string text = (num < 1f) ? num.ToString("0.##") : ((!(num < 10f)) ? num.ToString("F0") : num.ToString("0.#"));
        string param = text;
        return t.T("Time.HoursShort", param);
    }

}