namespace UnstableCoreChallenge.Specs;

public record UnstableCoreChallengeStage
{

    [Serialize]
    public int MinCycle { get; init; }

    [Serialize]
    public int MaxBombs { get; init; }

    [Serialize]
    public MinMaxSpec<int> PaymentEntries { get; init; } = new();

    [Serialize]
    public float ScienceChance { get; init; }

    [Serialize]
    public MinMaxSpec<int>? SciencePayment { get; init; }

    [Serialize]
    public ImmutableArray<ChallengeStagePayment?> Payments { get; init; } = [];

    [Serialize]
    public int DaysMin { get; init; }

    [Serialize]
    public int DaysMax { get; init; }

}

public record ChallengeStagePayment
{
    [Serialize]
    public int MinCount { get; init; }

    [Serialize]
    public int MaxCount { get; init; }

    [Serialize]
    public int MinAmount { get; init; }

    [Serialize]
    public int MaxAmount { get; init; }
}