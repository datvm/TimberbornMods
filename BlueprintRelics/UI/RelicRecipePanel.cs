namespace BlueprintRelics.UI;

[BindTransient]
public class RelicRecipePanel(ILoc t, IContainer container) : VisualElement
{

    public void SetRecipe(BlueprintRelicRecipePair specPair)
    {
        var (relicRecipeSpec, recipe) = specPair;

        this.AddGameLabel(t.T("LV.BRe.RecipeTitle",
            t.T("LV.BRe.Rarity_" + relicRecipeSpec.Rarity),
            t.T(recipe.DisplayLocKey)));

        if (relicRecipeSpec.BuildingName is not null)
        {
            this.AddGameLabel(relicRecipeSpec.BuildingName.Value);
        }

        this.AddChild(container.GetInstance<RecipeDescriptorPanel>)
            .SetRecipe(recipe);
    }

}
