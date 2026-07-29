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
    public int Order { get; init; }

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

    /// <summary>
    /// Optional unit-grid column override within the authored row.
    /// Default -1 = pack left→right by Order on that row only (other rows may reuse columns).
    /// </summary>
    [Serialize]
    public int X { get; init; } = -1;

    /// <summary>
    /// Unit-grid row. Same Y = same horizontal band. Default -1 = unplaced
    /// (overflow first row so missing layout is visible).
    /// </summary>
    [Serialize]
    public int Y { get; init; } = -1;

    /// <summary>
    /// Empty unit columns to leave immediately left of this tech on its row
    /// (after previous item / pack start). Default 0. Use e.g. 1 to separate groups.
    /// </summary>
    [Serialize]
    public int LeftX { get; init; }

    public bool ShouldAutoUnlock => Cost == 0;

    /// <summary>Has an authored row; column may be auto (depth) or forced via X.</summary>
    public bool HasAuthoredRow => Y >= 0;

    public bool HasForcedColumn => X >= 0;

}
