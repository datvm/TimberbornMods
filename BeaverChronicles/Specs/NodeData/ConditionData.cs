
namespace BeaverChronicles.Specs.NodeData;

public record ConditionData
{
    public string? FulfilledNodeId { get; init; }
    public string? FailedNodeId { get; init; }

    public ImmutableArray<ConditionItem> Conditions { get; init; } = [];
    public ConditionType ConditionType { get; init; }
}

public record ConditionItem
{
    public ConditionItemType Type { get; init; }
    public JObject? Parameters { get; init; }

    public T? GetParameters<T>() where T : class => Parameters?.ToObject<T>();

}

public enum ConditionItemType
{
    HasBuildings,
}