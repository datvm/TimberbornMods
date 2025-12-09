namespace ModdablePrefab;
public record PrefabAddComponentSpec : BasePrefabModSpec
{

    [Serialize]
    public ImmutableArray<string> AddComponents { get; init; }

    internal ImmutableArray<Type> AddComponentTypes { get; set; }

}
