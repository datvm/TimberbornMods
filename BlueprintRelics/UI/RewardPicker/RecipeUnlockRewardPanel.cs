namespace BlueprintRelics.UI.RewardPicker;

[BindTransient]
public class RecipeUnlockRewardPanel(ILoc t, IContainer container) : RewardPanelBase<RecipeUnlockUpgrade>(t)
{

    protected override void InitializeUI()
    {
        this.AddChild(container.GetInstance<RelicRecipePanel>)
            .SetRecipe(Reward.Spec);
    }

}
