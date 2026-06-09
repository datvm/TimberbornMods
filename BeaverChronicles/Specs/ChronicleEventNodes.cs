namespace BeaverChronicles.Specs;

public record class ChronicleEventNodeSpec
{
    [Serialize]
    public string Id { get; init; } = "";

    [Serialize]
    public string Type { get; init; } = "";

    [Serialize]
    public SerializedObject Data { get; init; } = null!;

    [Serialize]
    public string? NextNodeId { get; init; }

    object? cachedData;
    public T GetData<T>()
    {
        if (cachedData is not null)
        {
            return cachedData is T c
                ? c
                : throw new InvalidDataException($"A previous attempt to deserialize data for node {Id} resulted in a different type. Expected {typeof(T)}, got {cachedData.GetType()}.");
        }

        cachedData = Data.DeserializeTo<T>() ?? throw new InvalidDataException($"Node {Id} is missing data.");
        return (T)cachedData;
    }

    [JsonIgnore]
    public bool IsConditionNode => Type == ConditionNodeHandler.NodeType;
}

public record ChronicleEventNodes
{
    [Serialize]
    public string? StartNodeId { get; init; }

    [Serialize]
    public ImmutableArray<ChronicleEventNodeSpec> Items { get; init; } = [];

    FrozenDictionary<string, ChronicleEventNodeSpec> nodesById = null!;

    public ChronicleEventNodeSpec this[string id] 
        => TryGetNode(id, out var node) ? node
            : throw new KeyNotFoundException($"Node with id {id} not found.");

    public bool TryGetNode(string id, [NotNullWhen(true)] out ChronicleEventNodeSpec? node)
        => nodesById.TryGetValue(id, out node);

    internal void Initialize()
    {
        nodesById = Items.ToFrozenDictionary(c => c.Id);
    }

}
