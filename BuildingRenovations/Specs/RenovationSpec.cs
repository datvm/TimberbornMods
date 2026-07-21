namespace BuildingRenovations.Specs;

public record RenovationSpec : ComponentSpec
{
    [Serialize]
    public string Id { get; init; } = null!;

    [Serialize]
    public string GroupId { get; init; } = null!;

    [Serialize]
    public int Order { get; init; }

    [Serialize]
    public string TitleLoc { get; init; } = null!;
    [Serialize(nameof(TitleLoc))]
    public LocalizedText Title { get; init; } = null!;

    [Serialize]
    public string? DescLoc { get; init; }
    public string? Description { get; set; }

    [Serialize]
    public string FlavorLoc { get; init; } = "";
    [Serialize(nameof(FlavorLoc))]
    public LocalizedText Flavor { get; init; } = null!;

    [Serialize]
    public ImmutableArray<GoodAmountSpec> Cost { get; init; } = [];

    [Serialize]
    public float Days { get; init; }

    [Serialize]
    public ImmutableArray<float> Parameters { get; init; } = [];
}
