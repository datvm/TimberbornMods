namespace BeaverChronicles.Specs.NodeData;

public record TimeLimitData
{
    public string? Days { get; init; }
    public string? Hours { get; init; }
    public ImmutableArray<FormattableGoodItem> Payments { get; init; } = [];
    public ImmutableArray<TimeLimitSubscriptionData> Subscriptions { get; init; } = [];
    public string? PanelTextLoc { get; init; }
    public string? PaidNodeId { get; init; }
}

public record TimeLimitSubscriptionData
{
    public string EventName { get; init; } = "";
    public string? NextNodeId { get; init; }
}
