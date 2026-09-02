namespace CraneHeads.Components;

public record CraneHeadTrebuchetSpec : ComponentSpec
{
    [Serialize]
    public int WeightLimit { get; init; }

    [Serialize]
    public ImmutableArray<GoodAmountSpec> LaunchCost { get; init; } = [];

    [Serialize]
    public int LaunchCostCapacity { get; init; }

    [Serialize]
    public float CooldownHours { get; init; }

    [Serialize]
    public string Turret { get; init; } = "";

    [Serialize]
    public string Barrel { get; init; } = "";
}
