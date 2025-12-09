namespace ModdablePrefab;

public abstract record BasePrefabModSpec : ComponentSpec
{

    [Serialize]
    public string ComponentType { get; init; } = null!;

    [Serialize]
    public ImmutableArray<string> PrefabNames { get; init; } = [];

    [Serialize]
    public ImmutableArray<string> Factions { get; init; } = [];

}
