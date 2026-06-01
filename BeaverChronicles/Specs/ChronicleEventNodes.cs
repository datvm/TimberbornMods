namespace BeaverChronicles.Specs;

public record class ChronicleEventNodeSpec
{
    [Serialize]
    public string Id { get; init; } = null!;

    [Serialize]
    public ChronicleEventNodeType Type { get; init; }

    [Serialize]
    public SerializedObject Data { get; init; } = null!;

    [Serialize]
    public string? NextNodeId { get; init; }

    public T GetData<T>()
        => Data.DeserializeTo<T>() ?? throw new InvalidDataException($"Node {Id} is missing data.");
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

    public void Initialize()
    {
        nodesById = Items.ToFrozenDictionary(c => c.Id);
    }

}
