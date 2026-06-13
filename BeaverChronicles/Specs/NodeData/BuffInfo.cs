namespace BeaverChronicles.Specs.NodeData;

public record BuffInfo
{
    public string BuffId { get; init; } = "";
    public string TitleLoc { get; init; } = "";
    public string DescLoc { get; init; } = "";
    public string? Days { get; init; }
    public EntityBuffCategory Category { get; init; } = EntityBuffCategory.LimitedTime;
}
