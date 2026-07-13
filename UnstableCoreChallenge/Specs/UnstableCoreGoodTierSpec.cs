namespace UnstableCoreChallenge.Specs;

public record UnstableCoreGoodTierSpec : ComponentSpec
{

    [Serialize]
    public int Tier { get; init; }

    [Serialize]
    public ImmutableArray<string> GoodIds { get; init; } = [];

}
