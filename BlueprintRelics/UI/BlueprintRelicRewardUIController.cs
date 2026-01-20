namespace BlueprintRelics.UI;

[BindSingleton]
public class BlueprintRelicRewardUIController(
    IEnumerable<IRewardGenerator> rewardGenerators,
    IContainer container,
    EntityService entityService,
    InputService inputService
) : IInputProcessor, ILoadableSingleton
{

    readonly ImmutableArray<IRewardGenerator> rewardGenerators = OrganizeGenerators(rewardGenerators);
    static ImmutableArray<IRewardGenerator> OrganizeGenerators(IEnumerable<IRewardGenerator> generators)
    {
        var arr = new IRewardGenerator[BlueprintRelicRecipeRegistry.AllSizes.Length];

        foreach (var g in generators)
        {
            arr[(int)g.Size] = g;
        }

        foreach (var s in BlueprintRelicRecipeRegistry.AllSizes)
        {
            if (arr[(int)s] is null)
            {
                throw new InvalidOperationException($"No {nameof(IRewardGenerator)} registered for blueprint relic size {s}.");
            }
        }

        return [.. arr];
    }

    public void Load()
    {
        inputService.AddInputProcessor(this);
    }

    public async void PickReward(BlueprintRelicCollector collector)
    {
        if (!collector.Finished)
        {
            throw new InvalidOperationException("Cannot pick reward from an unfinished blueprint relic collector.");
        }

        var rewards = rewardGenerators[(int)collector.Size].GenerateRewards(collector);

        var diag = container.GetInstance<RelicRewardDialog>();
        diag.SetRewards(rewards);
        diag.OnUnlockedDialogRequested += ShowRelicUnlockDialog;

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

    public async void ShowRelicUnlockDialog()
    {
        await container.GetInstance<RelicUpgradesDialog>().ShowAsync();
    }

    public bool ProcessInput()
    {
        if (inputService.IsKeyDown("OpenRelicUnlockDialog"))
        {
            ShowRelicUnlockDialog();
            return true;
        }

        return false;
    }

}
