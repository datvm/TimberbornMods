namespace BlueprintRelics.UI;

public class BlueprintRewardUIController(
    BlueprintRelicRecipeUpgradeService upgradeService,
    IEnumerable<IRewardGenerator> rewardGenerators,
    IContainer container,
    EntityService entityService
)
{
    public static readonly ImmutableArray<BlueprintRelicSize> AllSizes = [..
        Enum.GetValues(typeof(BlueprintRelicSize))
        .Cast<BlueprintRelicSize>()
        .OrderBy(q => q)];

    readonly ImmutableArray<IRewardGenerator> rewardGenerators = OrganizeGenerators(rewardGenerators);
    static ImmutableArray<IRewardGenerator> OrganizeGenerators(IEnumerable<IRewardGenerator> generators)
    {
        var arr = new IRewardGenerator[AllSizes.Length];

        foreach (var g in generators)
        {
            arr[(int)g.ForSize] = g;
        }

        foreach (var s in AllSizes)
        {
            if (arr[(int)s] is null)
            {
                throw new InvalidOperationException($"No {nameof(IRewardGenerator)} registered for blueprint relic size {s}.");
            }
        }

        return [.. arr];
    }

    public async void PickReward(BlueprintRelicCollector collector)
    {
        if (!collector.Finished)
        {
            throw new InvalidOperationException("Cannot pick reward from an unfinished blueprint relic collector.");
        }

        var rewards = rewardGenerators[(int)collector.Size].GenerateRewards(collector, upgradeService.UpgradeSpec);

        var diag = container.GetInstance<RelicRewardDialog>();
        diag.SetRewards(rewards);

        var selected = await diag.ShowDialogAsync();
        if (selected is null)
        {
            collector.DigAgain();
        }
        else
        {
            selected.Apply();
            entityService.Delete(collector);
        }
    }

}
