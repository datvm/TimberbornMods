namespace ModdablePrefab;

public interface IPrefabModder
{

    ImmutableHashSet<Type> PrefabTypes { get; }
    void ModifyPrefab<T>(T prefab) where T : BaseComponent;

}
