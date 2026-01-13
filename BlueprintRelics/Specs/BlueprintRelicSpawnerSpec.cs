namespace BlueprintRelics.Specs;

public record BlueprintRelicSpawnerSpec : ComponentSpec
{

    [Serialize]
    public int MaxRelics { get; init; }

    [Serialize]
    public float BaseChance { get; init; }

    [Serialize]
    public float ChanceMultiplierPerRelic { get; init; }

    [Serialize]
    public ImmutableArray<int> SizeChanceWeight { get; init; }

}
