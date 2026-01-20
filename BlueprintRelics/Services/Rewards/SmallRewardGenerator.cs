namespace BlueprintRelics.Services.Rewards;

[MultiBind(typeof(IRewardGenerator))]
public class SmallRewardGenerator(BlueprintRelicRecipeUpgradeService upgradeService, BlueprintRelicRecipeService recipeService, ScienceService scienceService) : RewardGeneratorBase(upgradeService, recipeService, scienceService)
{
    public override BlueprintRelicSize Size => BlueprintRelicSize.Small;
    protected override int SpecialRewardChance => UpgradeSpec.CapacityRewardChance;

    protected override IRelicReward GetSpecialReward()
        => new RecipeCapacityUpgradeReward(upgradeService.GetRandomUnlockedRecipe(), upgradeService);
}

[MultiBind(typeof(IRewardGenerator))]
public class MediumRewardGenerator(BlueprintRelicRecipeUpgradeService upgradeService, BlueprintRelicRecipeService recipeService, ScienceService scienceService) : RewardGeneratorBase(upgradeService, recipeService, scienceService)
{
    public override BlueprintRelicSize Size => BlueprintRelicSize.Medium;
    protected override int SpecialRewardChance => UpgradeSpec.TimeReductionRewardChance;
    protected override IRelicReward GetSpecialReward() 
        => new RecipeTimeReductionUpgradeReward(upgradeService.GetRandomUnlockedRecipe(), upgradeService);
}

[MultiBind(typeof(IRewardGenerator))]
public class LargeRewardGenerator(BlueprintRelicRecipeUpgradeService upgradeService, BlueprintRelicRecipeService recipeService, ScienceService scienceService) : RewardGeneratorBase(upgradeService, recipeService, scienceService)
{
    public override BlueprintRelicSize Size => BlueprintRelicSize.Large;
    protected override int SpecialRewardChance => UpgradeSpec.AdditionalOutputRewardChance;

    protected override IRelicReward GetSpecialReward()
    {
        RecipeSpec recipe;
        ImmutableArray<GoodAmountSpec> products;
        do
        {
            recipe = upgradeService.GetRandomUnlockedRecipe();
            products = recipe.Products;
            if (products.Length > 0) { break; }
        } while (true);
        
        var productId = products[Random.RandomRangeInt(0, products.Length)].Id;

        return new RecipeOutputUpgradeReward(productId, recipe, upgradeService);
    }
}