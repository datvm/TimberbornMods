namespace BeaverChronicles.Specs.NodeData;

public record FindEntitiesData
{
    public bool AllBuildings { get; init; }
    public string[] TemplateNames { get; init; } = [];
    public string[] TemplatePrefixes { get; init; } = [];
    public CharacterType CharacterType { get; init; } = CharacterType.Unknown;
    public ImmutableArray<SerializableBoundsInts> Areas { get; init; } = [];
    public AreaCondition AreaCondition { get; init; } = AreaCondition.Intersects;
    public int MaxCount { get; init; } = 1;
    public bool ChooseRandom { get; init; }
    public string ResultName { get; init; } = null!;
    public string? NoneFoundNodeId { get; init; }

    [JsonIgnore]
    public ImmutableArray<BoundsInt> AreasBounds => field == default ? field = [.. Areas.Select(a => (BoundsInt)a)] : field;
}
