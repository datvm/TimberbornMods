namespace UnstableCoreChallenge.Specs;

public record CoreDisarmSpec : ComponentSpec
{
    [Serialize]
    public int Order { get; init; }
    [Serialize]
    public int EntryCount { get; init; }
    [Serialize]
    public float MaxCoreCount { get; init; }
    [Serialize]
    public int MinDays { get; init; }
    [Serialize]
    public int MaxDays { get; init; }
    [Serialize]
    public int MinScience { get; init; }
    [Serialize]
    public int MaxScience { get; init; }
    [Serialize]
    public ImmutableArray<GoodAmountSpec> Goods { get; init; }
}
