namespace ScenarioEditor.Services.ScenarioEvents;

public class ScenarioEventManager(
    ISingletonLoader loader
) : ILoadableSingleton, ISaveableSingleton
{
    static readonly SingletonKey SaveKey = new(nameof(ScenarioEditor));
    static readonly ListKey<ScenarioEvent> EventsKey = new("Events");

    internal Dictionary<int, ScenarioEvent> events = [];

    public void Load()
    {
        LoadSavedData();
    }

    void LoadSavedData()
    {
        if (!loader.TryGetSingleton(SaveKey, out var s)) { return; }

        if (s.Has(EventsKey))
        {
            events = s.Get(EventsKey, ScenarioEventSaver.Instance)
                .ToDictionary(q => q.Id);
        }
    }

    public void Save(ISingletonSaver singletonSaver)
    {
        var s = singletonSaver.GetSingleton(SaveKey);

        s.Set(EventsKey, events.Values, ScenarioEventSaver.Instance);
    }

}
