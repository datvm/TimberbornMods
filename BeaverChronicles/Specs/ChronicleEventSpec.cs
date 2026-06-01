namespace BeaverChronicles.Specs;

public record ChronicleEventSpec : ComponentSpec
{
    [Serialize]
    public string Id { get; init; } = null!;

    [Serialize]
    public string TitleLoc { get; init; } = null!;

    [Serialize(nameof(TitleLoc))]
    public LocalizedText Title { get; init; } = null!;

    [Serialize]
    public bool Repeat { get; init; }

    [Serialize]
    public ChronicleEventConditions Conditions { get; init; } = null!;

    [Serialize]
    public ChronicleEventParameters Parameters { get; init; } = null!;

    [Serialize]
    public ChronicleEventNodes Nodes { get; init; } = null!;

    public bool NeedCustomCode => Conditions.NeedCustomCode;

}
