namespace BeaverChronicles.Services;

[BindSingleton]
public class AreaEventService(
    ChronicleEventRegistry registry,
    ChronicleEventHistoryService history,
    AreaSegmentService areaSegmentService,
    DefaultEntityTracker<CharacterCellTracker> cellTrackers,
    EventBus eb
) : ILoadableSingleton
{
    readonly Dictionary<CharacterType, List<AreaEventEntry>?[]> events = [];
    readonly Dictionary<string, HashSet<AreaEventLocation>> eventLocations = [];

    bool modifierLock;
    readonly List<(IAreaChronicleEvent Event, bool Removing)> lockPendings = [];

    public void Load()
    {
        foreach (var c in BeaverChroniclesUtils.AllCharacters)
        {
            events[c] = new List<AreaEventEntry>?[areaSegmentService.SegmentsCount];
        }

        foreach (var ev in registry.EventById.Values.OfType<IAreaChronicleEvent>())
        {
            if (history.HasFinished(ev.Id)) { continue; }
            AddEventNow(ev);
        }

        history.EventFinished += OnEventFinished;
        history.EventRestored += OnEventRestored;

        cellTrackers.OnEntityRegistered += OnEntityRegistered;
    }

    void OnEntityRegistered(CharacterCellTracker obj) => obj.OnCellChanged += (_) => OnCharacterMoved(obj);

    void OnCharacterMoved(CharacterCellTracker obj) => RunWithLock(() =>
    {
        var cell = obj.Cell;
        var segment = obj.Segment;

        foreach (var entry in GetEventsFor(obj.CharacterType, segment))
        {
            if (!entry.Event.AreasActive) { continue; }
            if (!entry.Bounds.Contains(cell)) { continue; }

            eb.Post(new OnCharacterEnteredAreaEvent(obj, obj.Position, entry.Event));
            break;
        }
    });

    void OnEventRestored(object sender, string id)
    {
        if (!registry.TryGet(id, out var e) || e is not IAreaChronicleEvent ae) { return; }

        if (modifierLock)
        {
            lockPendings.Add((ae, false));
        }
        else
        {
            AddEventNow(ae);
        }
    }

    void OnEventFinished(object sender, string id)
    {
        if (!registry.TryGet(id, out var e) || e is not IAreaChronicleEvent ae) { return; }

        if (modifierLock)
        {
            lockPendings.Add((ae, true));
        }
        else
        {
            RemoveEventNow(ae.Id);
        }
    }

    void AddEventNow(IAreaChronicleEvent ev)
    {
        RemoveEventNow(ev.Id);

        var cs = ev.CharacterType;
        HashSet<AreaEventLocation> locations = [];
        foreach (var c in BeaverChroniclesUtils.AllCharacters)
        {
            if ((cs & c) == 0) { continue; }

            foreach (var area in ev.Areas)
            {
                foreach (var segment in area.Segments)
                {
                    if (segment < 0 || segment >= areaSegmentService.SegmentsCount) { continue; }

                    var buckets = events[c];
                    var bucket = buckets[segment];
                    bucket ??= buckets[segment] = [];
                    bucket.Add(new(ev, area.Bounds));
                    locations.Add(new(c, segment));
                }
            }
        }

        if (locations.Count > 0)
        {
            eventLocations[ev.Id] = locations;
        }
    }

    void RemoveEventNow(string id)
    {
        if (!eventLocations.Remove(id, out var locations)) { return; }

        foreach (var location in locations)
        {
            events[location.CharacterType][location.Segment]?.RemoveAll(e => e.Event.Id == id);
        }
    }

    IEnumerable<AreaEventEntry> GetEventsFor(CharacterType characterType, int segment)
    {
        if (!events.TryGetValue(characterType, out var buckets)) { yield break; }
        if (segment < 0 || segment >= buckets.Length) { yield break; }

        var bucket = buckets[segment];
        if (bucket is null) { yield break; }

        foreach (var entry in bucket)
        {
            yield return entry;
        }
    }

    void RunWithLock(Action action)
    {
        try
        {
            modifierLock = true;
            action();
        }
        finally
        {
            if (lockPendings.Count > 0)
            {
                foreach (var (ev, removing) in lockPendings)
                {
                    if (removing)
                    {
                        RemoveEventNow(ev.Id);
                    }
                    else
                    {
                        AddEventNow(ev);
                    }
                }
                lockPendings.Clear();
            }

            modifierLock = false;
        }
    }

    readonly record struct CharacterPair(BaseComponent Character, Transform Transform);
    readonly record struct AreaEventEntry(IAreaChronicleEvent Event, BoundsInt Bounds);
    readonly record struct AreaEventLocation(CharacterType CharacterType, int Segment);

}
