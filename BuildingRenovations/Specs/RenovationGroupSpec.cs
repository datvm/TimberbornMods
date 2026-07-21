namespace BuildingRenovations.Specs;

public record RenovationGroupSpec : ComponentSpec
{
    [Serialize]
    public string Id { get; init; } = null!;

    [Serialize]
    public string TitleLoc { get; init; } = null!;
    [Serialize(nameof(TitleLoc))]
    public LocalizedText Title { get; init; } = null!;

    [Serialize]
    public int Order { get; init; }
}
