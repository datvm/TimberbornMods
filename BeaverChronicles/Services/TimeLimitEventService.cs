namespace BeaverChronicles.Services;

[BindSingleton]
public class TimeLimitEventService(IEnumerable<ITimeLimitEvent> evs) : ILoadableSingleton
{

    FrozenDictionary<string, ITimeLimitEvent> events = null!;

    public void Load()
    {
        Dictionary<string, ITimeLimitEvent> dict = [];

        foreach (var e in evs)
        {
            foreach (var t in e.ForEvents)
            {
                if (dict.TryGetValue(t, out var existing))
                {
                    throw new Exception($"Duplicate time limit event for event {t}: {existing.GetType().FullName} and {e.GetType().FullName}");
                }

                dict[t] = e;
            }
        }

        events = dict.ToFrozenDictionary();
    }

    public void Subscribe(string name, SpecChronicleEventController controller)
    {
        if (events.TryGetValue(name, out var e))
        {
            e.SubscribeEvent(name, controller);
        }
        else
        {
            throw new InvalidOperationException($"Unknown time limit event: {name}");
        }
    }

    public void Unsubscribe(string name, SpecChronicleEventController controller)
    {
        if (events.TryGetValue(name, out var e))
        {
            e.UnsubscribeEvent(name, controller);
        }
        else
        {
            throw new InvalidOperationException($"Unknown time limit event: {name}");
        }
    }

}
