namespace ModdablePrefab;

public abstract record BasePrefabModSpec : ComponentSpec
{

    [Serialize]
    public string ComponentType { get; init; } = null!;

    [Serialize(true)]
    public ImmutableArray<string> PrefabNames { get; init; }

}
