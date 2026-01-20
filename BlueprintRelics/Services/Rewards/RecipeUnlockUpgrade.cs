namespace BlueprintRelics.Services.Rewards;

public class RecipeUnlockUpgrade(
    BlueprintRelicRecipePair spec,
    BlueprintRelicRecipeService recipeService
) : IRelicReward
{
    public BlueprintRelicRecipePair Spec { get; } = spec;
    public string TitleLoc => "LV.BRe.RewardUnlock";

    public void Apply()
    {
        recipeService.Unlock(Spec.Recipe.Id);
    }
}
