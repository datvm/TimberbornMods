namespace BeaverChronicles.Specs.NodeData;

public record BuffWorkplacesData : WorkplaceTargetData
{
    public string BuffId { get; init; } = "";
    public string TitleLoc { get; init; } = "";
    public string DescLoc { get; init; } = "";
    public ImmutableArray<FormattableGoodItem> Bonuses { get; init; } = [];
    public string? Days { get; init; }
}
