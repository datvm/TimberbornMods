namespace BeaverChronicles.Specs.NodeData;

public record TimeLimitData
{
    public float Hours { get; init; }
    public ImmutableArray<GoodAmountSpec> Payments { get; init; } = [];
    public string? PanelTextLoc { get; init; }
    public string? PaidNodeId { get; init; }
}
