namespace BlueprintRelics.UI.UpgradesDialog;

[BindTransient]
public class UnlockedRecipesPanel : CollapsiblePanel
{

    public UnlockedRecipesPanel(BlueprintRelicRecipeService recipeService, ILoc t, IContainer container)
    {
        SetTitle(t.T("LV.BRe.UnlockRecipes"));

        var parent = Container;
        parent.AddChild(container.GetInstance<RecipeStatPanel>);

        var list = parent.AddChild();

        var recipes = recipeService.GetUnlockedRecipes();
        if (recipes.Count <= 0)
        {
            list.AddGameLabel(t.T("LV.BRe.NoRecipes"));
            return;
        }

        foreach (var recipe in recipeService.GetUnlockedRecipes())
        {
            list.AddChild(container.GetInstance<RelicRecipePanel>)
                .SetMarginBottom(10)
                .SetRecipe(recipe);
        }
    }

}
