namespace BlueprintRelics.Services.Rewards;

public abstract class RewardGeneratorBase(
    BlueprintRelicRecipeUpgradeService upgradeService,
    BlueprintRelicRecipeService relicRecipeService,
    ScienceService scienceService) : IRewardGenerator
{
    protected readonly BlueprintRelicRecipeUpgradeService upgradeService = upgradeService;
    public BlueprintRelicRecipeUpgradeSpec UpgradeSpec => upgradeService.UpgradeSpec;

    public abstract BlueprintRelicSize Size { get; }
    protected abstract int SpecialRewardChance { get; }
    protected abstract IRelicReward GetSpecialReward();

    public IRelicReward[] GenerateRewards(BlueprintRelicCollector collector)
    {
        var spec = upgradeService.UpgradeSpec;

        var hasSpecialReward = SpecialRewardChance >= 100 || Random.RandomRangeInt(0, 100) < SpecialRewardChance;
        var recipeCount = spec.MaximumChoices - (hasSpecialReward ? 1 : 0);
        var recipes = GetRecipes(recipeCount, collector.RecipeRarityChance);

        if (recipes.Count < recipeCount) // Ran out of recipes
        {
            hasSpecialReward = true;
        }
        var hasScienceReward = recipes.Count == 0;

        return [.. Generator()];

        IEnumerable< IRelicReward> Generator()
        {
            foreach (var recipe in recipes)
            {
                yield return new RecipeUnlockUpgrade(recipe, relicRecipeService);
            }
            
            if (hasScienceReward)
            {
                yield return new ScienceReward(collector, scienceService);
            }

            if (hasSpecialReward)
            {
                yield return GetSpecialReward();
            }
        }
    }

    List<BlueprintRelicRecipePair> GetRecipes(int count, ImmutableArray<int> recipeRarityChance)
    {
        var recipes = relicRecipeService.GetLockedRecipesByRarity();

        List<BlueprintRelicRecipePair> result = [];
        for (int i = 0; i < count; i++)
        {
            var rarity = GetRandomIndex(recipeRarityChance);

            while (recipes[rarity].Count == 0 && rarity > 0) { rarity--; }
            
            var currRecipes = recipes[rarity];
            if (currRecipes.Count == 0) { break; } // No more recipes available

            var recipeIndex = Random.RandomRangeInt(0, currRecipes.Count);
            result.Add(currRecipes[recipeIndex]);
            currRecipes.RemoveAt(recipeIndex);
        }

        return result;
    }

    static int GetRandomIndex(ImmutableArray<int> weightedChances)
    {
        var total = weightedChances.Sum();
        var weight = Random.RandomRangeInt(0, total);

        for (int i = 0; i < weightedChances.Length; i++)
        {
            weight -= weightedChances[i];
            if (weight < 0)
            {
                return i;
            }
        }

        return -1; // Should never reach here
    }

}