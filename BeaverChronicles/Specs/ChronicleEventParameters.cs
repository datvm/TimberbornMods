namespace BeaverChronicles.Specs;

public record ChronicleEventParameters
{

    [Serialize]
    public ImmutableArray<string> Strings { get; init; } = [];

    [Serialize]
    public ImmutableArray<int> Ints { get; init; } = [];

    [Serialize]
    public ImmutableArray<float> Floats { get; init; } = [];

}
