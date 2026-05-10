namespace BuildingSigns.Specs;

public record BuildingSignsSpec : ComponentSpec
{
    [Serialize]
    public ImmutableArray<BuildingSignSpec> Signs { get; init; } = [];
}

public record BuildingSignSpec
{

    [Serialize]
    public string Id { get; init; } = null!;

    [Serialize]
    public string DisplayNameLoc { get; init; } = null!;

    [Serialize(nameof(DisplayNameLoc))]
    public LocalizedText DisplayName { get; init; } = null!;

    [Serialize]
    public Direction3D Face { get; init; }

    [Serialize]
    public TextAlignment HorizontalPosition { get; init; } = TextAlignment.Center;

    [Serialize]
    public TextAlignment VerticalPosition { get; init; } = TextAlignment.Center;

    [Serialize]
    public float DeltaX { get; init; } = 0;

    [Serialize]
    public float DeltaY { get; init; } = 0;

    [Serialize]
    public float Width { get; init; } = 1;

    [Serialize]
    public float Height { get; init; } = 1;

    [Serialize]
    public bool DefaultOn { get; init; }

    [Serialize]
    public int FontSize { get; init; } = 30;

}