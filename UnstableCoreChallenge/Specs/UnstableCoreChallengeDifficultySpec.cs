namespace UnstableCoreChallenge.Specs;

public record UnstableCoreChallengeDifficultySpec : ComponentSpec
{

    [Serialize]
    public string Id { get; init; } = null!;

    [Serialize]
    public int Order { get; init; }

    [Serialize]
    public bool IsDefault { get; init; }

    [Serialize]
    public string NameLoc { get; init; } = null!;
    [Serialize(nameof(NameLoc))]
    public LocalizedText Name { get; init; } = null!;

    [Serialize]
    public ImmutableArray<UnstableCoreChallengeStage> Stages { get; init; } = [];
    
}
