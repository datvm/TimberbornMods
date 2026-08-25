namespace CraneHeads.Components;

public record CraneHeadTrebuchetSpec : ComponentSpec
{
    [Serialize]
    public int WeightLimit { get; init; }

    [Serialize]
    public ImmutableArray<GoodAmountSpec> LaunchCost { get; init; } = [];

    [Serialize]
    public int LaunchCostCapacity { get; init; }
}
