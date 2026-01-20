namespace BlueprintRelics.Services.Rewards;

public abstract class RecipeUpgradeRewardBase(
    RecipeSpec recipeSpec,
    BlueprintRelicRecipeUpgradeService service
) : IRelicReward
{
    protected readonly BlueprintRelicRecipeUpgradeService service = service;
    public RecipeSpec RecipeSpec { get; } = recipeSpec;

    public abstract string TitleLoc { get; }

    public abstract void Apply();
}

public class RecipeCapacityUpgradeReward(RecipeSpec recipeSpec, BlueprintRelicRecipeUpgradeService service) : RecipeUpgradeRewardBase(recipeSpec, service)
{
    public override string TitleLoc { get; } = "LV.BRe.RewardCapacity";
    public override void Apply() => service.UpgradeCapacity(RecipeSpec.Id);
}

public class RecipeTimeReductionUpgradeReward(RecipeSpec recipeSpec, BlueprintRelicRecipeUpgradeService service) : RecipeUpgradeRewardBase(recipeSpec, service)
{
    public override string TitleLoc { get; } = "LV.BRe.RewardTimeReduction";
    public override void Apply() => service.UpgradeTimeReduction(RecipeSpec.Id);
}

public class RecipeOutputUpgradeReward(
    string outputId,
    RecipeSpec recipeSpec,    
    BlueprintRelicRecipeUpgradeService service
) : RecipeUpgradeRewardBase(recipeSpec, service)
{
    public override string TitleLoc { get; } = "LV.BRe.RewardOutput";
    public string OutputId { get; } = outputId;
    public override void Apply() => service.UpgradeOutput(RecipeSpec.Id, OutputId);
}
