namespace TimberPipes.Models;

public static class PipeFluids
{
    public const float PipeCapacity = 1f;
    public const float PacketVolume = 0.2f;
    public const float FullEpsilon = 0.01f;
    public const float MoveEpsilon = 1e-4f;
    public const string ContaminatedId = "_CONTAMINATED_PIPE_";
    public const string LiquidGoodType = "Liquid";
}

public readonly record struct PipeContaminationCause(string? GoodA, string? GoodB)
{
    public bool HasPair
        => GoodA is { Length: > 0 } && GoodB is { Length: > 0 } && GoodA != GoodB;
}
