namespace BlueprintRelics.Services.Rewards;

public abstract class RecipeUpgradeBase(
    RecipeSpec recipeSpec,
    BlueprintRelicRecipeUpgradeService service,
    ILoc t
) : IRelicReward
{
    protected readonly BlueprintRelicRecipeUpgradeService service = service;
    public RecipeSpec RecipeSpec { get; } = recipeSpec;

    public abstract string TitleLoc { get; }
    public string Title => t.T(TitleLoc);

    public abstract void Apply();
}

public class RecipeCapacityUpgrade(RecipeSpec recipeSpec, BlueprintRelicRecipeUpgradeService service, ILoc t) : RecipeUpgradeBase(recipeSpec, service, t)
{
    public override string TitleLoc { get; } = "LV.BRe.RewardCapacity";
    public override void Apply() => service.UpgradeCapacity(RecipeSpec.Id);
}

public class RecipeTimeReductionUpgrade(RecipeSpec recipeSpec, BlueprintRelicRecipeUpgradeService service, ILoc t) : RecipeUpgradeBase(recipeSpec, service, t)
{
    public override string TitleLoc { get; } = "LV.BRe.RewardTimeReduction";
    public override void Apply() => service.UpgradeTimeReduction(RecipeSpec.Id);
}

public class RecipeOutputUpgrade(
    string outputId,
    RecipeSpec recipeSpec,    
    BlueprintRelicRecipeUpgradeService service,
    ILoc t
) : RecipeUpgradeBase(recipeSpec, service, t)
{
    public override string TitleLoc { get; } = "LV.BRe.RewardOutput";
    public string OutputId { get; } = outputId;
    public override void Apply() => service.UpgradeOutput(RecipeSpec.Id, OutputId);
}
