namespace BlueprintRelics.UI.RewardPicker;

public abstract class RecipeUpgradeRewardPanel<T>(
    ILoc t,
    IContainer container
) : RewardPanelBase<T>(t)
    where T : RecipeUpgradeRewardBase
{
    protected override void InitializeUI()
    {
        var upgraded = GetUpgradedSpec(Reward.RecipeSpec);

        this.AddGameLabel(t.T(Reward.RecipeSpec.DisplayLocKey).Bold()).SetMarginBottom(10);

        this.AddGameLabel(t.T("LV.BRe.RecipeUpgrading"));
        var upgradedPanel = this.AddChild(container.GetInstance<RecipeDescriptorPanel>).SetMarginBottom();
        ShowRecipe(upgradedPanel, upgraded);

        this.AddGameLabel(t.T("LV.BRe.RecipeCurrent"));
        this.AddChild(container.GetInstance<RecipeDescriptorPanel>)
            .SetRecipe(Reward.RecipeSpec);
    }

    protected abstract RecipeSpec GetUpgradedSpec(RecipeSpec curr);
    protected abstract void ShowRecipe(RecipeDescriptorPanel panel, RecipeSpec recipe);
    protected abstract string GetDescription();
}

[BindTransient]
public class RecipeCapacityUpgradeRewardPanel(BlueprintRelicRecipeUpgradeService upgradeService, ILoc t, IContainer container) : RecipeUpgradeRewardPanel<RecipeCapacityUpgradeReward>(t, container)
{
    protected override string GetDescription() => t.T("LV.BRe.CapacityUpgradeDesc", upgradeService.UpgradeSpec.CapacityIncrease);

    protected override RecipeSpec GetUpgradedSpec(RecipeSpec curr) => curr with
    {
        CyclesCapacity = curr.CyclesCapacity + upgradeService.GetNextAdditionalCapacity(curr),
    };

    protected override void ShowRecipe(RecipeDescriptorPanel panel, RecipeSpec recipe)
        => panel.SetRecipe(recipe, highlightCapacity: true);
}

[BindTransient]
public class RecipeTimeReductionUpgradeRewardPanel(BlueprintRelicRecipeUpgradeService upgradeService, ILoc t, IContainer container) : RecipeUpgradeRewardPanel<RecipeTimeReductionUpgradeReward>(t, container)
{
    protected override string GetDescription() => t.T("LV.BRe.TimeReductionDesc", upgradeService.UpgradeSpec.TimeReduction);
    protected override RecipeSpec GetUpgradedSpec(RecipeSpec curr) => curr with
    {
        CycleDurationInHours = upgradeService.GetNextTimeAfterReduction(curr),
    };
    protected override void ShowRecipe(RecipeDescriptorPanel panel, RecipeSpec recipe)
        => panel.SetRecipe(recipe, highlightTime: true);
}

[BindTransient]
public class RecipeOutputUpgradeRewardPanel(BlueprintRelicRecipeUpgradeService upgradeService, ILoc t, IContainer container) : RecipeUpgradeRewardPanel<RecipeOutputUpgradeReward>(t, container)
{
    protected override string GetDescription() => t.T("LV.BRe.OutputIncreaseDesc", upgradeService.UpgradeSpec.AdditionalOutput);

    protected override RecipeSpec GetUpgradedSpec(RecipeSpec curr) => curr with
    {
        Products = [.. upgradeService.GetOutputUpgradeInfo(curr, Reward.OutputId)]
    };

    protected override void ShowRecipe(RecipeDescriptorPanel panel, RecipeSpec recipe)
        => panel.SetRecipe(recipe, highlightGood: Reward.OutputId);
}