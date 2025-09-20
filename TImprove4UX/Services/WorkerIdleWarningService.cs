namespace TImprove4UX.Services;

public class WorkerIdleWarningService(
    ISingletonLoader loader,
    EntityRegistry entities
) : ILoadableSingleton, ISaveableSingleton
{
    static readonly SingletonKey SaveKey = new(nameof(WorkerIdleWarningService));
    static readonly ListKey<string> DisabledPrefabsKey = new("DisabledPrefabs");

    HashSet<string> disabledPrefabs = [];

    public bool IsWarningDisabled(string prefab) => disabledPrefabs.Contains(prefab);
    public void ToggleWarningDisabled(string prefab, bool disabled)
    {
        if (disabled)
        {
            disabledPrefabs.Add(prefab);
        }
        else
        {
            disabledPrefabs.Remove(prefab);
        }

        foreach (var entity in entities.Entities)
        {
            var prefabSpec = entity.GetComponentFast<PrefabSpec>();
            if (!prefabSpec || prefabSpec.Name != prefab) { continue; }

            var warningComp = entity.GetComponentFast<WorkerIdleWarningComponent>();
            if (!warningComp) { continue; }

            warningComp.UpdateStatus(disabled);
        }
    }

    public void Load()
    {
        LoadSavedData();
    }

    void LoadSavedData()
    {
        if (!loader.TryGetSingleton(SaveKey, out var s)) { return; }

        if (s.Has(DisabledPrefabsKey))
        {
            disabledPrefabs = [..s.Get(DisabledPrefabsKey)];
        }
    }

    public void Save(ISingletonSaver singletonSaver)
    {
        if (disabledPrefabs.Count == 0) { return; }

        var s = singletonSaver.GetSingleton(SaveKey);
        s.Set(DisabledPrefabsKey, disabledPrefabs);
    }
}
