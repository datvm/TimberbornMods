namespace BuildingRenovations.Components;

/// <summary>
/// In-progress work timer for the current renovation (after materials are ready).
/// Uses absolute end <see cref="IDayNightCycle.PartialDayNumber"/> — no TimeTrigger.
/// </summary>
[AddTemplateModule2(typeof(BuildingRenovationComponent))]
public class RenovationWorkComponent(BuildingRenovationService service) : TickableComponent, IPersistentEntity, IAwakableComponent
{
    static readonly ComponentKey SaveKey = new(nameof(RenovationWorkComponent));
    static readonly PropertyKey<float> EndTimeKey = new("EndTime");
    static readonly PropertyKey<float> DurationKey = new("Duration");
    static readonly PropertyKey<string> RenovationIdKey = new("RenovationId");

    BuildingRenovationComponent controller = null!;

    float? endTime;
    float duration;
    string? renovationId;

    public bool IsWorking => endTime is not null;
    public string? RenovationId => renovationId;
    public float? EndTime => endTime;
    public float DurationDays => duration;

    public float DaysLeft
        => endTime is null ? 0f : Math.Max(0f, endTime.Value - service.PartialDayNumber);

    public float Progress
    {
        get
        {
            if (endTime is null || duration <= 0) { return 1f; }
            var left = DaysLeft;
            return Math.Clamp(1f - left / duration, 0f, 1f);
        }
    }

    public void Awake()
    {
        controller = GetComponent<BuildingRenovationComponent>();
        DisableComponent();
    }

    /// <param name="progress">0–1 work already done (for load).</param>
    public void Start(string id, float days, float progress = 0f)
    {
        Cancel();

        renovationId = id;
        duration = Math.Max(0f, days);

        if (duration <= 0 || progress >= 1f)
        {
            Finish();
            return;
        }

        var remaining = duration * (1f - Math.Clamp(progress, 0f, 1f));
        endTime = service.PartialDayNumber + remaining;
        EnableComponent();
    }

    public void FinishNow()
    {
        if (!IsWorking) { return; }
        Finish();
    }

    public void Cancel()
    {
        endTime = null;
        duration = 0;
        renovationId = null;
        DisableComponent();
    }

    public override void Tick()
    {
        if (endTime is null) {
            DisableComponent();
            return;
        }

        if (service.PartialDayNumber < endTime.Value) { return; }

        Finish();
    }

    void Finish()
    {
        var id = renovationId;
        Cancel();
        if (id is not null)
        {
            controller.OnWorkCompleted(id);
        }
    }

    public void Save(IEntitySaver entitySaver)
    {
        if (!IsWorking || renovationId is null || endTime is null) { return; }

        var s = entitySaver.GetComponent(SaveKey);
        s.Set(EndTimeKey, endTime.Value);
        s.Set(DurationKey, duration);
        s.Set(RenovationIdKey, renovationId);
    }

    public void Load(IEntityLoader entityLoader)
    {
        if (!entityLoader.TryGetComponent(SaveKey, out var s)) { return; }
        if (!s.Has(EndTimeKey) || !s.Has(RenovationIdKey)) { return; }

        endTime = s.Get(EndTimeKey);
        duration = s.Has(DurationKey) ? s.Get(DurationKey) : 0f;
        renovationId = s.Get(RenovationIdKey);
        EnableComponent();
    }
}
