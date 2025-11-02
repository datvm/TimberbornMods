namespace UnstableCoreChallenge.Services;

public class CoreDisarmService(
    ISpecService specs,
    IGoodService goods,
    MSettings s,
    GameCycleService cycles
) : ILoadableSingleton
{
    static readonly float[] DifficultyCycleMultiplier = [3f, 2f, 1.5f];
    public const float GoodsMinimumFactor = 0.5f;
    public const int ScienceChance = 10;

    ImmutableArray<CoreDisarmSpec> cycleSpecs = [];
    FrozenDictionary<string, int> endlessGoods = FrozenDictionary<string, int>.Empty;
    CoreDisarmSpec endlessSpec = null!;

    public int EntriesCount { get; private set; }
    public KeyValuePair<int, int> ScienceCost { get; private set; }
    public int MaxCoreCount { get; private set; }
    public KeyValuePair<int, int> Days { get; private set; }
    public ImmutableArray<StabilizerGood> StabilizerGoods { get; private set; }

    public void Load()
    {
        InitSpecs();
        CalculateForThisCycle();
    }

    void InitSpecs()
    {
        var allGoods = specs.GetSpecs<GoodSpec>().Select(q => q.Id).ToHashSet();

        List<CoreDisarmSpec> cycleSpecs = [];
        HashSet<int> orders = [];
        foreach (var spec in specs.GetSpecs<CoreDisarmSpec>().OrderBy(q => q.Order))
        {
            // Validate
            if (!orders.Add(spec.Order))
            {
                throw new ArgumentException($"Duplicate CoreDisarmSpec order value '{spec.Order}' found in '{spec.Blueprint.Name}'");
            }

            if (endlessSpec is null && spec.Order != -1)
            {
                throw new ArgumentException($"CoreDisarmSpec '{spec.Blueprint.Name}' has Order '{spec.Order}' but endless spec (Order = -1) must be defined first");
            }

            foreach (var g in spec.Goods)
            {
                if (!allGoods.Contains(g.Id))
                {
                    throw new ArgumentOutOfRangeException($"CoreDisarmSpec '{spec.Blueprint.Name}' references unknown GoodSpec '{g.Id}'");
                }
            }

            if (spec.Goods.Length < spec.EntryCount)
            {
                throw new ArgumentException($"CoreDisarmSpec '{spec.Blueprint.Name}' has EntryCount '{spec.EntryCount}' greater than available Goods '{spec.Goods.Length}'");
            }

            if (spec.Order == -1)
            {
                endlessSpec = spec;
            }
            else
            {
                cycleSpecs.Add(spec);
            }
        }

        if (endlessSpec is null)
        {
            throw new MissingMemberException("No CoreDisarmSpec found for endless mode (Order = -1)");
        }

        if (cycleSpecs.Count == 0)
        {
            throw new MissingMemberException("No CoreDisarmSpec found for normal cycles (Order >= 0)");
        }

        this.cycleSpecs = [.. cycleSpecs.OrderBy(q => q.Order)];
        endlessGoods = endlessSpec.Goods.ToFrozenDictionary(q => q.Id, q => q.Amount);
    }

    public void CalculateForThisCycle()
    {
        var cycle = cycles.Cycle;

        var (spec, endlessCount) = GetActiveSpec(cycle);
        EntriesCount = spec.EntryCount;
        var maxCoreCount = spec.MaxCoreCount;

        var minScience = spec.MinScience;
        var maxScience = spec.MaxScience;

        var minDay = spec.MinDays;
        var maxDay = spec.MaxDays;

        var stabilizerGoods = spec.Goods
            .Where(q => goods.HasGood(q.Id) && q.Amount > 0)
            .ToDictionary(q => q.Id, q => q.Amount);

        if (endlessCount > 0)
        {
            var endless = endlessSpec;

            minScience += endless.MinScience * endlessCount;
            maxScience += endless.MaxScience * endlessCount;

            minDay = Math.Max(3, minDay + endless.MinDays * endlessCount);
            maxDay = Math.Max(3, maxDay + endless.MaxDays * endlessCount);

            maxCoreCount += endless.MaxCoreCount * endlessCount;

            foreach (var (g, amount) in stabilizerGoods)
            {
                if (endlessGoods.TryGetValue(g, out var endlessAmount))
                {
                    stabilizerGoods[g] = amount + endlessAmount * endlessCount;
                }
            }
        }

        MaxCoreCount = Mathf.FloorToInt(maxCoreCount);
        ScienceCost = new(minScience, maxScience);
        Days = new(minDay, maxDay);
        StabilizerGoods = [.. stabilizerGoods.Select(q => new StabilizerGood(
            q.Key,
            Mathf.CeilToInt(q.Value * GoodsMinimumFactor),
            q.Value))];

        TimberUiUtils.LogVerbose(() => $"""
        
        [{nameof(UnstableCoreChallenge)}]
        Cycle {cycle}, Endless = {endlessCount}:
        - Max Core Count: {MaxCoreCount}
        - Science Cost: {ScienceCost.Key} - {ScienceCost.Value}
        - Days: {Days.Key} - {Days.Value}
        - Entries Count: {EntriesCount}
        """);
    }

    public StabilizerRecord Generate()
    {
        var days = RandomInclusive(Days.Key, Days.Value);

        var goodsCount = EntriesCount;
        int? science = null;
        if (ScienceCost.Value > 0 && RandomInclusive(0, 99) < ScienceChance)
        {
            goodsCount -= 1;
            science = RandomInclusive(ScienceCost.Key, ScienceCost.Value);
        }

        var goods = GenerateGoods(goodsCount);
        return new(days, science, goods);
    }

    ImmutableArray<GoodAmount> GenerateGoods(int count)
    {
        if (count <= 0) { return []; }

        var indices = new HashSet<int>();
        if (StabilizerGoods.Length <= count)
        {
            indices.AddRange(Enumerable.Range(0, StabilizerGoods.Length));
        }
        else
        {
            while (indices.Count < count)
            {
                indices.Add(RandomInclusive(0, StabilizerGoods.Length - 1));
            }
        }

        return [..indices.Select(i => {
            var g = StabilizerGoods[i];
            var amount = RandomInclusive(g.Min, g.Max);
            return new GoodAmount(g.Id, amount);
        })];
    }

    (CoreDisarmSpec spec, int endless) GetActiveSpec(int cycle)
    {
        var diff = s.DifficultyValue;

        // Increasing difficulty cycle start points for each difficulty (when diffCycle increments by 1)
        // Formula: diffCycle = Floor((cycle - 1) / m), where m = DifficultyCycleMultiplier[diff]
        // The cycle at which diffCycle becomes k (k >= 1) is: ceil(k * m + 1)
        // Examples with current multipliers:
        // - diff = 0 (m = 3.0): 4, 7, 10, 13, ...
        // - diff = 1 (m = 2.0): 3, 5, 7, 9, ...
        // - diff = 2 (m = 1.5): 3, 4, 6, 7, 9, 10, ...

        var diffCycle = Mathf.FloorToInt(Math.Max(1, cycle - 1) / DifficultyCycleMultiplier[diff]);

        if (diffCycle < cycleSpecs.Length)
        {
            return (cycleSpecs[diffCycle], 0);
        }

        // Endless mode
        return (cycleSpecs[^1], s.EndlessMode.Value ? diffCycle - cycleSpecs.Length + 1 : 0);
    }

    static int RandomInclusive(int min, int max)
        => min == max ? min : UnityEngine.Random.RandomRangeInt(min, max + 1);

}
public record StabilizerGood(string Id, int Min, int Max);
public record StabilizerRecord(int Days, int? Science, ImmutableArray<GoodAmount> Goods);