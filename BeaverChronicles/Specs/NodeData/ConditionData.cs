
namespace BeaverChronicles.Specs.NodeData;

public record ConditionData
{
    public string? FulfilledNodeId { get; init; }
    public string? FailedNodeId { get; init; }

    public ImmutableArray<ConditionItem> Conditions { get; init; } = [];
    public ConditionType ConditionType { get; init; } = ConditionType.All;
}

public record ConditionItem
{
    public string Type { get; init; } = "";
    public JObject? Parameters { get; init; }

    object? cachedParameters;
    public T? GetParameters<T>() where T : class
    {
        if (cachedParameters is not null)
        {
            return cachedParameters is T c 
                ? c
                : throw new InvalidOperationException($"A previous call to GetParameters returned a value of type {cachedParameters.GetType().FullName}, which cannot be cast to {typeof(T).FullName}.");
        }

        var result = Parameters?.ToObject<T>();
        cachedParameters = result;
        return result;
    }
}