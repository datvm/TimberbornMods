namespace BeaverChronicles.Specs;

public record ChronicleEventConditions
{

    [Serialize]
    public ImmutableArray<EventTriggerSource> Sources { get; init; } = [];

    [Serialize]
    public ImmutableArray<string> RequiredFlags { get; init; } = [];
    [Serialize]
    public ConditionType RequiredFlagsCondition { get; init; } = ConditionType.Any;

    [Serialize]
    public ImmutableArray<string> RequiredNoFlags { get; init; } = [];
    [Serialize]
    public ConditionType RequiredNoFlagsCondition { get; init; } = ConditionType.Any;

    [Serialize]
    public ImmutableArray<string> BlockedFlags { get; init; } = [];
    [Serialize]
    public ConditionType BlockedFlagsCondition { get; init; } = ConditionType.Any;

    [Serialize]
    public int Weight { get; init; }
    [Serialize]
    public bool CustomWeightCode { get; init; }

    public bool NeedCustomCode => CustomWeightCode;

}