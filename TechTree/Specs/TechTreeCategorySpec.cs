namespace TechTree.Specs;

public record TechTreeCategorySpec : ComponentSpec
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
    public int Order { get; init; }

    [Serialize]
    public Color BackgroundColor { get; init; } = Color.white;

    [Serialize]
    public Color ItemBorderColor { get; init; } = Color.black;

    [Serialize]
    public Color ItemBackgroundColor { get; init; } = Color.clear;

    [Serialize]
    public Color ItemTextColor { get; init; } = Color.black;

}
