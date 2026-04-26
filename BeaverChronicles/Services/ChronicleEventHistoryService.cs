namespace BeaverChronicles.Services;

[BindSingleton]
public class ChronicleEventHistoryService(
    ISingletonLoader loader,
    IDayNightCycle dayNightCycle
) : ISaveableSingleton, ILoadableSingleton
{
    static readonly SingletonKey SaveKey = new(nameof(ChronicleEventHistoryService));
    static readonly ListKey<string> RecordsKey = new("Records");
    static readonly PropertyKey<float> NextEventMinimumDayKey = new("NextEventMinimumDay");
    static readonly ListKey<string> FinishedEventIdsKey = new("FinishedEventIds");
    static readonly PropertyKey<string> NextEventIdRequestedKey = new("NextEventIdRequested");

    readonly List<EventHistoryRecord> records = [];
    readonly Dictionary<string, List<EventHistoryRecord>> recordsById = [];
    
    public IReadOnlyList<EventHistoryRecord> Records => records;
    public HashSet<string> FinishedEventIds { get; } = [];
    public string? NextEventIdRequested { get; set; }

    public string? ActiveEventId
    {
        get
        {
            var lastEv = records.LastOrDefault();
            return lastEv is not null && lastEv.EndDay is null ? lastEv.Id : null;
        }
    }

    public float NextEventMinimumDay { get; private set; }

    public void Load()
    {
        LoadSavedData();

        if (records.Count > 0)
        {
            PopulateLookupData();
        }
    }

    public IReadOnlyList<EventHistoryRecord> Get(string id) 
        => recordsById.TryGetValue(id, out var list) ? list : [];

    public void StartEvent(string id)
    {
        var lastEv = records.LastOrDefault();
        if (lastEv is not null && lastEv.EndDay is null)
        {
            throw new InvalidOperationException("The last recorded event has not ended yet.");
        }

        EventHistoryRecord newRecord = new(id, dayNightCycle.PartialDayNumber, null);
        records.Add(newRecord);
        recordsById.GetOrAdd(id, () => []).Add(newRecord);
    }

    public void EndEvent()
    {
        if (records.Count == 0)
        {
            throw new InvalidOperationException("There is no recorded event yet.");
        }

        var r = records[^1];
        if (r.EndDay is not null)
        {
            throw new InvalidOperationException("The last recorded event has already ended.");
        }

        var updatedRecord = r with { EndDay = dayNightCycle.PartialDayNumber, };
        recordsById[r.Id][^1] = records[^1] = updatedRecord;
    }

    public void RequestNextEventDelay(float days = 0) 
        => NextEventMinimumDay = days <= 0 ? 0 : dayNightCycle.PartialDayNumber + days;

    void LoadSavedData()
    {
        if (!loader.TryGetSingleton(SaveKey, out var s)) { return; }

        records.AddRange(s.Get(RecordsKey).Select(EventHistoryRecord.Deserialize));
        NextEventMinimumDay = s.Get(NextEventMinimumDayKey);
        FinishedEventIds.UnionWith(s.Get(FinishedEventIdsKey));

        if (s.Has(NextEventIdRequestedKey))
        {
            NextEventIdRequested = s.Get(NextEventIdRequestedKey);
        }
    }

    void PopulateLookupData()
    {
        foreach (var r in records)
        {
            recordsById.GetOrAdd(r.Id, () => []).Add(r);
        }
    }

    public void Save(ISingletonSaver singletonSaver)
    {
        var s = singletonSaver.GetSingleton(SaveKey);
        s.Set(RecordsKey, [.. records.Select(r => r.Serialize())]);
        s.Set(NextEventMinimumDayKey, NextEventMinimumDay);
        s.Set(FinishedEventIdsKey, FinishedEventIds);

        if (NextEventIdRequested is not null)
        {
            s.Set(NextEventIdRequestedKey, NextEventIdRequested);
        }
    }

}
