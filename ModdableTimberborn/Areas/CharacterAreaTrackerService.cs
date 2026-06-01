namespace ModdableTimberborn.Areas;

public class CharacterAreaTrackerService(
    CharacterTracker characterTracker,
    AreaSegmentService segmentService
) : ILoadableSingleton
{
    readonly DeferredHashSet<CharacterAreaTrackerHandle>[] handles
        = new DeferredHashSet<CharacterAreaTrackerHandle>[(int)CharacterType.ArrayLength];
    readonly bool[] listening = new bool[(int)CharacterType.ArrayLength];

    DeferredHashSet<CharacterAreaTrackerHandle>?[] handlesInSegments = [];

    public void Load()
    {
        foreach (var c in ModdableTimberbornUtils.AllCharacterTypes)
        {
            handles[(int)c] = [];
        }

        handlesInSegments = new DeferredHashSet<CharacterAreaTrackerHandle>[segmentService.SegmentsCount];

        characterTracker.OnEntityRegistered += OnEntityRegistered;
        characterTracker.OnEntityUnregistered += OnEntityUnregistered;
    }

    public CharacterAreaTrackerHandle RegisterArea(CharacterAreaTrackerRegistration registration)
    {
        var handle = new CharacterAreaTrackerHandle(registration, segmentService);

        foreach (var s in handle.Segments)
        {
            var bucket = handlesInSegments[s] ??= [];
            bucket.Add(handle);
        }

        var chars = handle.CharacterTypes;
        PerformOnBuckets(chars, (i, h) =>
        {
            h.Add(handle);

            if (!listening[i])
            {
                RegisterListening(i);
            }
        });

        handle.Initialize(GetTrackersForType(chars));

        return handle;
    }

    public void UnregisterArea(CharacterAreaTrackerHandle handle)
    {
        foreach(var s in handle.Segments)
        {
            var bucket = handlesInSegments[s];
            bucket?.Remove(handle);
        }

        PerformOnBuckets(handle.CharacterTypes, (i, h) =>
        {
            h.Remove(handle);

            if (h.Count == 0 && listening[i])
            {
                UnregisterListening(i);
            }
        });
    }

    void RegisterListening(int index)
    {
        if (listening[index]) { return; }
        listening[index] = true;

        foreach (var c in GetTrackersForType((CharacterType)index))
        {
            c.OnCellChanged += OnCellChanged;
        }
    }

    void UnregisterListening(int index)
    {
        if (!listening[index]) { return; }
        listening[index] = false;

        foreach (var c in GetTrackersForType((CharacterType)index))
        {
            c.OnCellChanged -= OnCellChanged;
        }
    }

    void OnEntityRegistered(CharacterTrackerComponent obj)
    {
        var type = obj.CharacterType;
        if (!listening[(int)type]) { return; }

        // No need to call OnNewEntity, the position is not updated anyway, just wait for the next tick
        obj.GetComponent<CharacterPositionTracker>().OnCellChanged += OnCellChanged;
    }

    void OnEntityUnregistered(CharacterTrackerComponent obj)
    {
        var tracker = obj.GetComponent<CharacterPositionTracker>();
        if (!tracker) { return; }

        tracker.OnCellChanged -= OnCellChanged;
        foreach (var bucket in handles[(int)obj.CharacterType])
        {
            bucket.OnEntityRemoved(tracker);
        }
    }

    void OnCellChanged(object sender, CharacterTrackedPositionChange e)
    {
        var c = (CharacterPositionTracker)sender;
        var(old, @new) = e;

        foreach (var h in GetHandlesFor(c.CharacterType, old.Segment, @new.Segment))
        {
            h.OnEntityUpdated(c);
        }
    }

    IEnumerable<CharacterPositionTracker> GetTrackersForType(CharacterType type) => characterTracker.GetCharacters<CharacterPositionTracker>(type);

    IEnumerable<CharacterAreaTrackerHandle> GetHandlesFor(CharacterType type, int segment1, int segment2)
    {
        var cHandles = handles[(int)type];
        if (cHandles.Count == 0) { yield break; }

        var handles1 = GetBucket(segment1);
        var handles2 = GetBucket(segment2);
        if (handles1.Count == 0 && handles2.Count == 0) { yield break; }

        foreach (var handle in cHandles)
        {
            if (handles1.Contains(handle) || handles2.Contains(handle))
            {
                yield return handle;
            }
        }

        DeferredHashSet<CharacterAreaTrackerHandle> GetBucket(int segment)
        {
            if (segment < 0 || segment >= handlesInSegments.Length) { return []; }
            return handlesInSegments[segment] ?? [];
        }
    }

    void PerformOnBuckets(CharacterType type, Action<int, DeferredHashSet<CharacterAreaTrackerHandle>> action)
    {
        foreach (var c in ModdableTimberbornUtils.AllCharacterTypes)
        {
            if ((type & c) != 0)
            {
                var index = (int)c;
                action(index, handles[index]);
            }
        }
    }

}
