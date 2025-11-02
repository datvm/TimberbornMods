namespace TImprove4Ui.Services;

public class PauseStatusIconRegistry(
    ISingletonLoader loader,
    EventBus eb
) : ILoadableSingleton, ISaveableSingleton, IUnloadableSingleton
{
    readonly record struct PauseStatusInstance(StatusTracker Tracker, StatusInstance PauseInstance);

    public static PauseStatusIconRegistry? Instance { get; private set; }

    static readonly SingletonKey SaveKey = new(nameof(PauseStatusIconRegistry));
    static readonly ListKey<string> DisabledPrefabsKey = new("DisabledPrefabs");

    readonly HashSet<string> disabledPrefabs = [];

    readonly HashSet<StatusInstance> disablingInstances = [];
    readonly Dictionary<string, List<PauseStatusInstance>> pauseInstancesByPrefab = [];

    public void Load()
    {
        Instance = this;
        LoadSavedData();
        eb.Register(this);
    }

    public bool HasPauseStatus(string prefab) => pauseInstancesByPrefab.ContainsKey(prefab);

    public bool ShouldDisable(StatusInstance instance) => disablingInstances.Contains(instance);

    public bool ShouldDisable(string prefabName) => disabledPrefabs.Contains(prefabName);

    public void AddDisabledPrefab(string prefab)
    {
        if (disabledPrefabs.Add(prefab))
        {
            ScanForStatus(prefab, true);
        }
    }

    public void RemoveDisabledPrefab(string prefab)
    {
        if (disabledPrefabs.Remove(prefab))
        {
            ScanForStatus(prefab, false);
        }
    }

    [OnEvent]
    public void OnEntityAdded(EntityInitializedEvent e)
    {
        var tracker = e.Entity.GetComponent<StatusTracker>();
        if (!tracker) { return; }

        tracker.StatusAdded += OnStatusAdded;
        foreach (var status in tracker.Statuses)
        {
            CheckForStatus(tracker, status);
        }
    }

    [OnEvent]
    public void OnEntityRemoved(EntityDeletedEvent e)
    {
        var tracker = e.Entity.GetComponent<StatusTracker>();
        if (!tracker) { return; }

        tracker.StatusAdded -= OnStatusAdded;
        foreach (var status in tracker.Statuses)
        {
            disablingInstances.Remove(status);
        }

        if (pauseInstancesByPrefab.TryGetValue(tracker.PrefabName, out var list))
        {
            list.RemoveAll(q => q.Tracker == tracker);
        }
    }

    void OnStatusAdded(object sender, StatusInstance e)
    {
        if (sender is not StatusTracker tracker) { return; }
        CheckForStatus(tracker, e);
    }

    void CheckForStatus(StatusTracker tracker, StatusInstance e)
    {
        if (e.StatusDescription != MenuLoaderService.PauseStatusDescription) { return; }

        pauseInstancesByPrefab.GetOrAdd(tracker.PrefabName).Add(new(tracker, e));
        if (ShouldDisable(tracker.PrefabName))
        {
            ConfigureInstance(e, true);
        }
    }

    void ScanForStatus(string prefab, bool disabled)
    {
        if (!pauseInstancesByPrefab.TryGetValue(prefab, out var list)) { return; }
        foreach (var pause in list)
        {
            ConfigureInstance(pause.PauseInstance, disabled);
        }
    }

    void ConfigureInstance(StatusInstance instance, bool disabled)
    {
        if (disabled)
        {
            disablingInstances.Add(instance);
        }
        else
        {
            disablingInstances.Remove(instance);
        }

        if (instance.IsActive)
        {
            instance.StatusSubject.UpdateStatus(instance, false);
            instance.StatusSubject.UpdateStatus(instance, true);
        }
    }

    void LoadSavedData()
    {
        if (!loader.TryGetSingleton(SaveKey, out var s)) { return; }

        if (s.Has(DisabledPrefabsKey))
        {
            disabledPrefabs.Clear();
            disabledPrefabs.AddRange(s.Get(DisabledPrefabsKey));
        }
    }

    public void Save(ISingletonSaver singletonSaver)
    {
        if (disabledPrefabs.Count == 0) { return; }

        var s = singletonSaver.GetSingleton(SaveKey);
        s.Set(DisabledPrefabsKey, [.. disabledPrefabs]);
    }

    public void Unload()
    {
        Instance = null;
    }
}