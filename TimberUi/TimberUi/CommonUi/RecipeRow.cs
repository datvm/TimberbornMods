namespace TimberUi.CommonUi;

[BindSingleton]
public class RecipeRowFactory(NamedIconProvider icons, IGoodService goods, ILoc t)
{

    public RecipeRow Create(RecipeSpec recipe) => new(recipe, icons, goods, t);

}

public class RecipeRow : VisualElement
{

    public ImmutableArray<IconSpan> Ingredients { get; } 
    public ImmutableArray<IconSpan> Products { get; } 
    public IconSpan Time { get; }
    public IconSpan? Fuel { get; }
    public IconSpan? Science { get; }

    public RecipeRow(RecipeSpec recipe, NamedIconProvider icons, IGoodService goods, ILoc t)
    {
        this.SetAsRow().AlignItems().JustifyContent();

        List<IconSpan> list = [];
        if (recipe.ConsumesIngredients || recipe.ConsumesFuel)
        {
            var ingr = this.AddRow().AlignItems();

            foreach (var i in recipe.Ingredients)
            {
                list.Add(ingr.AddIconSpan()
                    .SetName(i.Id)
                    .SetGood(goods, i.Id, i.Amount.ToString()));
            }

            if (recipe.ConsumesFuel)
            {
                Fuel = ingr.AddIconSpan()
                    .SetName(recipe.Fuel)
                    .SetGood(goods, recipe.Fuel, (1f / recipe.CyclesFuelLasts).ToString("0.##"));
            }

            this.AddArrow(icons);
        }
        Ingredients = [.. list];

        Time = this.AddIconSpan()
            .SetName("Time")
            .SetTime(icons, t.T("Time.HoursShort", recipe.CycleDurationInHours.ToString("0.##")));

        list.Clear();
        if (recipe.ProducesProducts || recipe.ProducesSciencePoints)
        {
            this.AddArrow(icons);

            var prod = this.AddRow().AlignItems();
            foreach (var p in recipe.Products)
            {
                list.Add(prod.AddIconSpan()
                    .SetName(p.Id)
                    .SetGood(goods, p.Id, p.Amount.ToString()));
            }

            if (recipe.ProducesSciencePoints)
            {
                Science = prod.AddIconSpan()
                    .SetName("Science")
                    .SetScience(icons, recipe.ProducedSciencePoints.ToString());
            }
        }
        Products = [.. list];
    }

}
