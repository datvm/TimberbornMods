namespace BeaverChronicles.Services;

[BindSingleton]
public class ChronicleEventRegistry(IEnumerable<IChronicleEvent> events) : ILoadableSingleton
{

#nullable disable
    public FrozenDictionary<EventTriggerSource, ImmutableArray<IChronicleEvent>> EventsByTrigger { get; private set; }
    public FrozenDictionary<string, IChronicleEvent> EventById { get; private set; }
#nullable enable

    public void Load()
    {
        Dictionary<string, IChronicleEvent> eventById = [];
        foreach (var e in events)
        {
            var id = e.Id;
            if (eventById.ContainsKey(id))
            {
                throw new InvalidOperationException($"Duplicate event id {id} by {e} and {eventById[e.Id]}");
            }

            eventById[e.Id] = e;
        }
        EventById = eventById.ToFrozenDictionary();

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
