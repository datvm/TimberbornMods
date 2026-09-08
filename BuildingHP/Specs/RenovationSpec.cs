namespace BuildingHP.Specs;

public record RenovationSpec : ComponentSpec, IOrderedSpec
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
    public string DescLoc { get; init; } = null!;
    public string Description { get; set; } = null!;

    [Serialize]
    public string FlavorLoc { get; init; } = null!;
    [Serialize(nameof(FlavorLoc))]
    public LocalizedText Flavor { get; init; } = null!;

    [Serialize]
    public ImmutableArray<GoodAmountSpecNew> Cost { get; init; } = [];

    [Serialize]
    public float Days { get; init; }

    [Serialize]
    public ImmutableArray<float> Parameters { get; init; } = [];

    [Serialize]
    public bool CannotCancel { get; init; }
}
