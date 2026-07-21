namespace BuildingRenovations.Components;

/// <summary>Chronological log of every renovation that finished on this building (newest last).</summary>
[AddTemplateModule2(typeof(BuildingRenovationComponent))]
public class RenovationRecordComponent(BuildingRenovationService service) : BaseComponent, IPersistentEntity
{
    static readonly ComponentKey SaveKey = new(nameof(RenovationRecordComponent));
    static readonly PropertyKey<string> RecordsKey = new("Records");

    readonly List<RenovationRecord> records = [];

    public IReadOnlyList<RenovationRecord> Records => records;

    /// <summary>Newest first.</summary>
    public IEnumerable<RenovationRecord> NewestFirst => records.AsEnumerable().Reverse();

    public void Add(string renovationId)
    {
        records.Add(new RenovationRecord(renovationId, service.PartialDayNumber));
    }

    public void Save(IEntitySaver entitySaver)
    {
        if (records.Count == 0) { return; }

        var s = entitySaver.GetComponent(SaveKey);
        s.Set(RecordsKey, JsonConvert.SerializeObject(records));
    }

    public void Load(IEntityLoader entityLoader)
    {
        if (!entityLoader.TryGetComponent(SaveKey, out var s) || !s.Has(RecordsKey)) { return; }

        var loaded = JsonConvert.DeserializeObject<List<RenovationRecord>>(s.Get(RecordsKey));
        records.Clear();
        if (loaded is null) { return; }

        records.AddRange(loaded);
    }
}

public record RenovationRecord(string Id, float Time);
