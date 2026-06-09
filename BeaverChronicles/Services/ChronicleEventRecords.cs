namespace BeaverChronicles.Services;

[BindSingleton]
public class ChronicleEventRecords(
    ISingletonLoader loader,
    IDayNightCycle dayNightCycle
) : ISaveableSingleton, ILoadableSingleton
{
    static readonly SingletonKey SaveKeyCompat = new("ChronicleEventHistoryService"); // Compatibility
    static readonly SingletonKey SaveKey = new(nameof(ChronicleEventRecords));

    static readonly ListKey<string> RecordsKey = new("Records");
    static readonly PropertyKey<float> NextEventMinimumDayKey = new("NextEventMinimumDay");
    static readonly ListKey<string> FinishedEventIdsKey = new("FinishedEventIds");
    static readonly PropertyKey<string> NextEventIdRequestedKey = new("NextEventIdRequested");
    static readonly PropertyKey<string> ActiveEventIdKey = new("ActiveEventId");

    readonly List<EventHistoryRecord> records = [];
    readonly Dictionary<string, List<EventHistoryRecord>> recordsById = [];

    public event EventHandler<string>? EventFinished;
    public event EventHandler<string>? EventRestored;

    public IReadOnlyList<EventHistoryRecord> Records => records;

    readonly HashSet<string> finishedEventIds = [];
    public IReadOnlyCollection<string> FinishedEventIds => finishedEventIds;

    public string? NextEventIdRequested { get; set; }

    string activeEventId = "";
    public string? ActiveEventId => string.IsNullOrEmpty(activeEventId) ? null : activeEventId;

    public EventHistoryRecord? ActiveRecord
        => ActiveEventId is { } id ? records.LastOrDefault(r => r.Id == id && r.EndDay is null) : null;

    public float NextEventMinimumDay { get; private set; }

    public void Load()
    {
        LoadSavedData();

        if (records.Count > 0)
        {
            PopulateLookupData();
        }
    }

    public bool HasFinished(string id) => finishedEventIds.Contains(id);
    public void MarkFinished(string id)
    {
        finishedEventIds.Add(id);
        EventFinished?.Invoke(this, id);
    }

    public void RestoreFinished(string id)
    {
        finishedEventIds.Remove(id);
        EventRestored?.Invoke(this, id);
    }

    public IReadOnlyList<EventHistoryRecord> Get(string id)
        => recordsById.TryGetValue(id, out var list) ? list : [];

    public int GetOccurrence(EventHistoryRecord record)
    {
        if (!recordsById.TryGetValue(record.Id, out var list))
        {
            throw new InvalidOperationException($"No records found for event {record.Id}.");
        }

        var index = list.IndexOf(record);
        if (index == -1)
        {
            throw new InvalidOperationException($"Record for event {record.Id} was not found in the history lookup.");
        }

        return index + 1;
    }

    public EventHistoryRecord StartEvent(string id, bool isMini)
    {
        if (!isMini && ActiveRecord is not null)
        {
            throw new InvalidOperationException("There is already an active event. Only mini event are allowed.");
        }

        EventHistoryRecord newRecord = new(id, dayNightCycle.PartialDayNumber, null);
        records.Add(newRecord);
        recordsById.GetOrAdd(id, () => []).Add(newRecord);
        if (!isMini)
        {
            activeEventId = id;
        }

        return newRecord;
    }

    public void EndEvent() => EndEvent(ActiveRecord ?? throw new InvalidOperationException("There is no active event."));

    public void EndEvent(EventHistoryRecord record)
    {
        if (record.EndDay is not null)
        {
            throw new InvalidOperationException("The event record has already ended.");
        }

        var activeRecord = ActiveRecord;
        var updatedRecord = record with { EndDay = dayNightCycle.PartialDayNumber, };
        records[records.LastIndexOf(record)] = updatedRecord;

        var recordsForId = recordsById[record.Id];
        recordsForId[recordsForId.LastIndexOf(record)] = updatedRecord;

        if (activeRecord == record)
        {
            activeEventId = "";
        }
    }

    public void RequestNextEventDelay(float days = 0)
        => NextEventMinimumDay = days <= 0 ? 0 : dayNightCycle.PartialDayNumber + days;

    void LoadSavedData()
    {
        if (!loader.TryGetSingleton(SaveKey, out var s)
            && !loader.TryGetSingleton(SaveKeyCompat, out s))
        {
            return;
        }

        records.AddRange(s.Get(RecordsKey).Select(EventHistoryRecord.Deserialize));
        NextEventMinimumDay = s.Get(NextEventMinimumDayKey);
        finishedEventIds.UnionWith(s.Get(FinishedEventIdsKey));

        activeEventId = s.Has(ActiveEventIdKey)
            ? s.Get(ActiveEventIdKey)
            : FindLegacyActiveEventId();

        if (s.Has(NextEventIdRequestedKey))
        {
            NextEventIdRequested = s.Get(NextEventIdRequestedKey);
        }
    }

    string FindLegacyActiveEventId()
        => records.LastOrDefault(r => r.EndDay is null)?.Id ?? "";

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
        s.Set(FinishedEventIdsKey, finishedEventIds);
        s.Set(ActiveEventIdKey, activeEventId);

        if (NextEventIdRequested is not null)
        {
            s.Set(NextEventIdRequestedKey, NextEventIdRequested);
        }
    }

}
