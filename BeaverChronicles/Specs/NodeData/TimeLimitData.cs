namespace BeaverChronicles.Specs.NodeData;

public record TimeLimitData
{
    public string? Days { get; init; }
    public string? Hours { get; init; }
    public ImmutableArray<FormattableGoodItem> Payments { get; init; } = [];
    public ImmutableArray<TimeLimitCustomTriggerData> CustomTriggers { get; init; } = [];
    public string? PanelTextLoc { get; init; }
    public string? PaidNodeId { get; init; }
}

public record TimeLimitCustomTriggerData
{
    public TimeLimitCustomTriggerInterval Interval { get; init; } = TimeLimitCustomTriggerInterval.Day;
    public string ConditionNodeId { get; init; } = "";
    public string? TriggerNodeId { get; init; }
}

public enum TimeLimitCustomTriggerInterval
{
    Tick,
    Hour,
    Day
}
