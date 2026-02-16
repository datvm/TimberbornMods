namespace BuildingBlueprints.Services;

[BindSingleton]
public class BlueprintGroupService(
    ISingletonLoader loader
) : ISaveableSingleton, ILoadableSingleton
{
    static readonly SingletonKey SaveKey = new(nameof(BlueprintGroupService));
    static readonly PropertyKey<int> CurrentGroupKey = new("CurrentGroup");

    int currGroup = 1;
    readonly Dictionary<int, HashSet<BuildingBlueprintComponent>> groups = [];

    public int GetNextGroup() => ++currGroup;

    public void Register(BuildingBlueprintComponent comp)
    {
        var id = comp.BlueprintGroup;
        if (id <= 0) { return; }

        groups.GetOrAdd(id, () => []).Add(comp);
    }

    public void Unregister(BuildingBlueprintComponent comp)
    {
        var id = comp.BlueprintGroup;
        if (id <= 0) { return; }

        var list = groups.GetOrDefault(id);
        if (list is null) { return; }

        list.Remove(comp);

        if (list.Count == 0)
        {
            groups.Remove(id);
        }
    }

    public IReadOnlyCollection<BuildingBlueprintComponent> GetGroup(int id) => groups.GetOrDefault(id) ?? [];

    public IReadOnlyCollection<BuildingBlueprintComponent> GetGroup(BuildingBlueprintComponent comp)
        => comp.HasGroup ? GetGroup(comp.BlueprintGroup) : [];

    public void Load()
    {
        if (!loader.TryGetSingleton(SaveKey, out var s)) { return; }
        currGroup = s.Get(CurrentGroupKey);
    }

    public void Save(ISingletonSaver singletonSaver)
    {
        var s = singletonSaver.GetSingleton(SaveKey);
        s.Set(CurrentGroupKey, currGroup);
    }

}
