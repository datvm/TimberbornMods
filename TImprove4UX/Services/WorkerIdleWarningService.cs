namespace TImprove4UX.Services;

public class WorkerIdleWarningService(
    ISingletonLoader loader,
    EntityRegistry entities,
    MSettings s
) : ILoadableSingleton, ISaveableSingleton
{
    static readonly SingletonKey SaveKey = new(nameof(WorkerIdleWarningService));
    static readonly ListKey<string> DisabledPrefabsKey = new("DisabledPrefabs");

    public readonly MSettings Settings = s;

    HashSet<string> disabledPrefabs = [];

    public bool IsWarningDisabled(string prefab) => !Settings.WorkerIdleWarning.Value || disabledPrefabs.Contains(prefab);
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
            var prefabSpec = entity.GetComponent<TemplateSpec>();
            if (prefabSpec is null || prefabSpec.TemplateName != prefab) { continue; }

            var warningComp = entity.GetComponent<WorkerIdleWarningComponent>();
            if (!warningComp) { continue; }

            warningComp.UpdateStatus(disabled);
        }
    }

    public void Load()
    {
        LoadSavedData();

        Settings.WorkerIdleWarning.ValueChanged += OnWorkerIdleWarningChanged;
        OnWorkerIdleWarningChanged(null!, Settings.WorkerIdleWarning.Value);
    }

    void OnWorkerIdleWarningChanged(object sender, bool enabled)
    {
        foreach (var e in entities.Entities)
        {
            var prefabSpec = e.GetComponent<TemplateSpec>();
            if (prefabSpec is null) { continue; }

            var warningComp = e.GetComponent<WorkerIdleWarningComponent>();
            if (!warningComp) { continue; }            

            var disabled = IsWarningDisabled(prefabSpec.TemplateName);
            warningComp.UpdateStatus(disabled);
        }
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
