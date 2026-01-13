namespace BlueprintRelics.Services.Rewards;

public class RecipeUnlockUpgrade(
    RecipeSpec recipeSpec,
    BlueprintRelicRecipeRarity rarity,
    ModdableRecipeLockService locker,
    ILoc t
) : IRelicReward
{
    public RecipeSpec RecipeSpec { get; } = recipeSpec;
    public BlueprintRelicRecipeRarity Rarity { get; } = rarity;
    public string Title => t.T("LV.BRe.RewardUnlock");

    public void Apply()
    {
        locker.Unlock(RecipeSpec.Id);
    }
}
