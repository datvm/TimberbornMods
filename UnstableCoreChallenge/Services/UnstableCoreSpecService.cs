namespace UnstableCoreChallenge.Services;

[BindSingleton]
public class UnstableCoreSpecService(
    ISpecService specs,
    MSettings s,
    IGoodService goodService
) : ILoadableSingleton
{
    public const string Science = "Science";

    public UnstableCoreChallengeDifficultySpec Difficulty { get; private set; } = null!;
    public ImmutableArray<ImmutableArray<string>> GoodTiers = [];

    int cachedCycle = -1;
    UnstableCoreChallengeStage? cachedStats;

    public void Load()
    {
        Difficulty = s.SelectedDifficulty;

        if (Difficulty.Stages.Length == 0)
        {
            throw new Exception($"No stages defined for difficulty: {Difficulty.Name}");
        }

        ParseGoods();
    }

    void ParseGoods()
    {
        var goodTierSpecs = specs.GetSpecs<UnstableCoreGoodTierSpec>().ToArray();
        var tiers = new ImmutableArray<string>[goodTierSpecs.Length];

        foreach (var spec in goodTierSpecs)
        {
            var tier = spec.Tier;
            if (tier < 0 || tier >= tiers.Length)
            {
                throw new Exception($"Invalid good tier: {tier}, {spec.Blueprint.Name}");
            }

            if (tiers[tier] != default)
            {
                throw new Exception($"Duplicate good tier: {tier}, {spec.Blueprint.Name}");
            }

            tiers[tier] = [.. spec.GoodIds
                .Where(id => id == Science || goodService.HasGood(id))];
        }

        // Here, each tier should already be filled, no extra validation is needed
        GoodTiers = [.. tiers];
    }

    public UnstableCoreChallengeStage GetStats(int cycle)
    {
        if (cachedStats is not null && cachedCycle == cycle)
        {
            return cachedStats;
        }

        cachedCycle = cycle;
        return cachedStats = GetStage(cycle);
    }

    UnstableCoreChallengeStage GetStage(int cycle)
    {
        var curr = Difficulty.Stages[0];
        foreach (var s in Difficulty.Stages)
        {
            if (s.MinCycle > cycle)
            {
                break;
            }

            curr = s;
        }

        return curr;
    }

}