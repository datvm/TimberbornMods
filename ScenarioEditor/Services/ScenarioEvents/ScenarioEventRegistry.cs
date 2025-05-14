namespace ScenarioEditor.Services.ScenarioEvents;

public class ScenarioEventRegistry(
    ScenarioEventManager scenarioEventManager,
    EventBus eb,
    IContainer container
) : ILoadableSingleton
{

#nullable disable
    FrozenDictionary<int, ScenarioEvent> events;
    HashSet<int> inprogressEvents = [];
#nullable enable

    public IReadOnlyCollection<int> InprogressEventsIds => inprogressEvents;
    public ImmutableArray<ScenarioEvent> InprogressEvents => [.. inprogressEvents.Select(id => events[id])];

    public void Load()
    {
        events = scenarioEventManager.events.ToFrozenDictionary();
        Inject();

        inprogressEvents = [.. events
            .Where(q => q.Value.IsInProgress)
            .Select(q => q.Key)];

        eb.Register(this);
    }

    void Inject()
    {
        foreach (var ev in events.Values)
        {
            foreach (var trigger in ev.Triggers)
            {
                container.Inject(trigger);
            }

            foreach (var action in ev.Actions)
            {
                container.Inject(action);
            }
        }
    }


    [OnEvent]
    public void OnScenarioEventChanged(OnScenarioEventChanged ev)
    {
        var id = ev.Event.Id;
        if (ev.Event.IsInProgress)
        {
            inprogressEvents.Add(id);
        }
        else
        {
            inprogressEvents.Remove(id);
        }
    }

}
