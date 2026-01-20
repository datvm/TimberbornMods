namespace BlueprintRelics.UI.UpgradesDialog;

[BindTransient]
public class UpgradedRecipesPanel : CollapsiblePanel
{
    static readonly StringBuilder desc = new();

    public UpgradedRecipesPanel(
        ILoc t,
        BlueprintRelicRecipeUpgradeService upgrades,
        RecipeSpecService recipes,
        IGoodService goodService,
        IContainer container
    )
    {
        SetTitle(t.T("LV.BRe.UpgradedRecipes"));

        var parent = Container;

        if (upgrades.Upgrades.Count == 0)
        {
            parent.AddGameLabel(t.T("LV.BRe.NoUpgrades"));
            return;
        }

        foreach (var (id, recipeUpgrades) in upgrades.Upgrades.OrderBy(q => q.Key))
        {
            var el = parent.AddChild().SetMarginBottom();

            var recipe = recipes.GetRecipe(id);

            desc.AppendLine(t.T(recipe.DisplayLocKey).Bold().Highlight());

            if (recipeUpgrades.CapacityUpgrades > 0)
            {
                desc.AppendLine(t.T("LV.BRe.RewardCapacity") + ": " + recipeUpgrades.CapacityUpgrades);
            }
            if (recipeUpgrades.TimeReductionUpgrades > 0)
            {
                desc.AppendLine(t.T("LV.BRe.RewardTimeReduction") + ": " + recipeUpgrades.TimeReductionUpgrades);
            }
            el.AddGameLabel(desc.ToStringWithoutNewLineEndAndClean());

            if (recipeUpgrades.OutputUpgrades.Count > 0)
            {
                var lstOutputs = el.AddRow().AlignItems();
                lstOutputs.AddGameLabel(t.T("LV.BRe.RewardOutput") + ":").SetMarginRight();

                foreach (var (g, n) in recipeUpgrades.OutputUpgrades)
                {
                    lstOutputs.AddIconSpan().SetGood(goodService, g, "+" + n.ToString());
                }
            }

            el.AddChild(container.GetInstance<RecipeDescriptorPanel>).SetRecipe(recipe);
        }
    }

}
