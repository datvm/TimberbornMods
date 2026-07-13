namespace TechTree.Specs;

public record TechTreeItemSpec : ComponentSpec
{

    [Serialize]
    public string Id { get; init; } = null!;

    [Serialize]
    public string NameLoc { get; init; } = null!;
    [Serialize(nameof(NameLoc))]
    public LocalizedText Name { get; init; } = null!;

    [Serialize]
    public string? DescriptionLoc { get; init; }
    [Serialize(nameof(DescriptionLoc))]
    public LocalizedText? Description { get; init; }

    [Serialize]
    public Sprite? Icon { get; init; }

    [Serialize]
    public string? CategoryId { get; init; }

    [Serialize]
    public int Cost { get; init; }

    [Serialize]
    public ImmutableArray<string> Prerequisites { get; init; } = [];

    [Serialize]
    public ImmutableArray<string> Tags { get; init; } = [];
}
