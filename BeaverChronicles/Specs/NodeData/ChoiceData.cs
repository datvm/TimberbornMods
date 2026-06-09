namespace BeaverChronicles.Specs.NodeData;

public record ChoiceData
{
    public string TextLoc { get; init; } = null!;
    public bool TopImage { get; init; }
    public string? TopImagePath { get; init; }
    public bool SideImage { get; init; }
    public string? SideImagePath { get; init; }
    public ImmutableArray<ChoiceOptionData> Options { get; init; } = [];
    public int DefaultOption { get; init; } = 1;

    public bool ReuseRecordPage { get; init; }
}

public record ChoiceOptionData
{
    public string TextLoc { get; init; } = null!;
    public string? NoteLoc { get; init; }
    public string? NextNodeId { get; init; }

    public string? EnabledConditionNodeId { get; init; }
    public string? DisabledTextLoc { get; init; }
}
