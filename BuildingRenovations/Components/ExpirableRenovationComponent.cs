namespace BuildingRenovations.Components;

[AddTemplateModule2(typeof(BuildingRenovationComponent))]
public class ExpirableRenovationComponent(BuildingRenovationService service)
    : TickableComponent, IPersistentEntity, IAwakableComponent
{
    static readonly ComponentKey SaveKey = new(nameof(ExpirableRenovationComponent));
    static readonly PropertyKey<string> EndTimesKey = new("EndTimes");

    readonly Dictionary<string, float> endTimes = [];

    BuildingRenovationComponent controller = null!;

    public IReadOnlyDictionary<string, float> EndTimes => endTimes;

    public void Awake()
    {
        controller = GetComponent<BuildingRenovationComponent>();
        DisableComponent();
    }

    public void Track(ExpirableRenovationBase expirable)
    {
        var days = expirable.GetDurationDays(controller);
        if (days <= 0)
        {
            ExpireNow(expirable.Id);
            return;
        }

        SetEndTime(expirable.Id, service.PartialDayNumber + days);
    }

    public void ResumeIfNeeded(ExpirableRenovationBase expirable)
    {
        if (endTimes.ContainsKey(expirable.Id))
        {
            UpdateEnabled();
            return;
        }

        Track(expirable);
    }

    public void SetEndTime(string id, float endPartialDay)
    {
        if (endPartialDay <= service.PartialDayNumber)
        {
            endTimes.Remove(id);
            ExpireNow(id);
            return;
        }

        endTimes[id] = endPartialDay;
        UpdateEnabled();
    }

    public void SetRemainingDays(string id, float days)
        => SetEndTime(id, service.PartialDayNumber + days);

    public void Extend(string id, float extraDays)
    {
        if (extraDays == 0) { return; }

        if (!endTimes.TryGetValue(id, out var end))
        {
            if (extraDays > 0)
            {
                SetRemainingDays(id, extraDays);
            }
            return;
        }

        SetEndTime(id, end + extraDays);
    }

    public void Reduce(string id, float days) => Extend(id, -days);

    /// <summary>Stop tracking without calling OnExpired (e.g. manual remove).</summary>
    public void Cancel(string id)
    {
        if (!endTimes.Remove(id)) { return; }
        UpdateEnabled();
    }

    public void ClearAll()
    {
        endTimes.Clear();
        UpdateEnabled();
    }

    public bool TryGetRemainingDays(string id, out float days)
    {
        if (!endTimes.TryGetValue(id, out var endTime))
        {
            days = 0;
            return false;
        }

        days = Math.Max(0, endTime - service.PartialDayNumber);
        return true;
    }

    public bool TryGetEndTime(string id, out float endTime)
        => endTimes.TryGetValue(id, out endTime);

    public override void Tick()
    {
        if (endTimes.Count == 0)
        {
            UpdateEnabled();
            return;
        }

        var now = service.PartialDayNumber;
        List<string>? expired = null;

        foreach (var (id, endTime) in endTimes)
        {
            if (now < endTime) { continue; }

            expired ??= [];
            expired.Add(id);
        }

        if (expired is null) { return; }

        foreach (var id in expired)
        {
            endTimes.Remove(id);
            ExpireNow(id);
        }

        UpdateEnabled();
    }

    void ExpireNow(string id)
    {
        endTimes.Remove(id);
        controller.OnRenovationExpired(id);
        UpdateEnabled();
    }

    void UpdateEnabled()
    {
        if (endTimes.Count > 0)
        {
            if (!Enabled)
            {
                EnableComponent();
            }
        }
        else if (Enabled)
        {
            DisableComponent();
        }
    }

    public void Save(IEntitySaver entitySaver)
    {
        if (endTimes.Count == 0) { return; }

        var s = entitySaver.GetComponent(SaveKey);
        s.Set(EndTimesKey, JsonConvert.SerializeObject(endTimes));
    }

    public void Load(IEntityLoader entityLoader)
    {
        if (!entityLoader.TryGetComponent(SaveKey, out var s)) { return; }
        if (!s.Has(EndTimesKey)) { return; }

        var loaded = JsonConvert.DeserializeObject<Dictionary<string, float>>(s.Get(EndTimesKey));
        endTimes.Clear();
        if (loaded is null) { return; }

        foreach (var (id, endTime) in loaded)
        {
            endTimes[id] = endTime;
        }

        UpdateEnabled();
    }
}
