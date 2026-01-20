namespace BlueprintRelics.UI;

[BindTransient]
public class RecipeStatPanel : VisualElement
{

    public RecipeStatPanel(ILoc t, BlueprintRelicRecipeService recipeService)
    {
        this.SetAsRow().AlignItems();

        var stats = recipeService.GetUnlockedStats();
        var (totalUnlocked, totalRecipes) = stats[^1];

        this.AddGameLabel($"{t.T("LV.BRe.UnlockRecipes")}: {totalUnlocked}/{totalRecipes}");

        foreach (var rarity in BlueprintRelicRecipeRegistry.AllRarities)
        {
            var (unlocked, total) = stats[(int)rarity];

            this.AddGameLabel($"{t.T("LV.BRe.Rarity_" + rarity)}: {unlocked}/{total}")
                .SetMargin(left: 20);
        }
    }

}
