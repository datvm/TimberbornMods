namespace BlueprintRelics.UI;

[BindTransient]
public class RecipeDescriptorPanel(
    ILoc t,
    IGoodService goodService,
    RecipeRowFactory recipeRowFactory
) : VisualElement
{

    public void SetRecipe(RecipeSpec recipe, 
        bool highlightCapacity = default,
        bool highlightTime = default,
        string? highlightGood = default
    )
    {
        var recipeRow = this.AddChild(() => recipeRowFactory.Create(recipe))
            .SetMarginBottom(10);

        if (highlightTime)
        {
            var lbl = recipeRow.Time.PostfixLabel!;
            lbl.text = lbl.text.Bold().Highlight();
        }

        if (highlightGood is not null)
        {
            foreach (var prod in recipeRow.Products)
            {
                if (prod.name == highlightGood)
                {
                    var lbl = prod.PrefixLabel!;
                    lbl.text = lbl.text.Bold().Highlight();
                }
            }
        }

        var capacityRow = this.AddRow().AlignItems().JustifyContent();
        AddCapacityGoods(capacityRow, recipe.CyclesCapacity, [.. recipe.Ingredients, .. recipe.Products], highlightCapacity);
        
        if (recipe.ConsumesFuel)
        {
            capacityRow.AddIconSpan().SetGood(goodService, recipe.Fuel)
                .SetPrefixText(t.T("LV.BRe.FuelCapacity", recipe.FuelCapacity));
        }
    }

    void AddCapacityGoods(VisualElement parent, int capacity, IEnumerable<GoodAmountSpec> goods, bool highlight)
    {
        var capacityText = capacity.ToString();
        if (highlight)
        {
            capacityText = capacityText.Highlight();
        }

        parent.AddGameLabel(t.T("LV.BRe.RecipeCapacity", capacityText)).SetMarginRight();

        foreach (var g in goods)
        {
            var amount = (g.Amount * capacity).ToString();
            if (highlight)
            {
                amount = amount.Highlight();
            }

            parent.AddIconSpan().SetGood(goodService, g.Id, amount);
        }
    }

}
