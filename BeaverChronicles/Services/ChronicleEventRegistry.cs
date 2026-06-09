namespace BeaverChronicles.Services;

[BindSingleton]
public class ChronicleEventRegistry(IEnumerable<IChronicleEventsProvider> eventsProviders) : ILoadableSingleton
{

    public FrozenDictionary<EventTriggerSource, ImmutableArray<IChronicleEvent>> EventsByTrigger { get; private set; } = null!;
    public FrozenDictionary<string, IChronicleEvent> EventById { get; private set; } = null!;
    public FrozenDictionary<string, IMiniChronicleEvent> MiniEventById { get; private set; } = null!;

    public void Load()
    {
        PopulateEvents();
        PopulateEventTriggers();
    }

    void PopulateEvents()
    {
        Dictionary<string, IChronicleEvent> eventById = [];
        Dictionary<string, IMiniChronicleEvent> miniEventById = [];

        foreach (var p in eventsProviders)
        {
            foreach (var e in p.GetEvents())
            {
                var id = e.Id;
                if (eventById.ContainsKey(id))
                {
                    throw new InvalidOperationException($"Duplicate event id {id} by {e} and {eventById[e.Id]}, provided by {p.GetType().Name}");
                }

                eventById[e.Id] = e;

                if (e is IMiniChronicleEvent mini)
                {
                    miniEventById[mini.Id] = mini;
                }
            }
        }

        EventById = eventById.ToFrozenDictionary();
        MiniEventById = miniEventById.ToFrozenDictionary();
    }

    void PopulateEventTriggers()
    {
        Dictionary<EventTriggerSource, HashSet<IChronicleEvent>> eventByTrigger = [];
        foreach (var src in BeaverChroniclesUtils.AllTriggerSources)
        {
            eventByTrigger[src] = [];
        }

        foreach (var e in EventById.Values)
        {
            foreach (var src in e.TriggerSources)
            {
                eventByTrigger[src].Add(e);
            }
        }

        EventsByTrigger = eventByTrigger.ToFrozenDictionary(kv => kv.Key, kv => kv.Value.ToImmutableArray());
    }

    public IChronicleEvent Get(string id) => EventById[id];

    public bool TryGet(string id, [NotNullWhen(true)] out IChronicleEvent? e) => EventById.TryGetValue(id, out e);

    public bool Has(string id) => EventById.ContainsKey(id);

}
