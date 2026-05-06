namespace ModdableTimberborn.UpdatableEntityStats;

public class PopulationStat(PopulationStatService service) : ComponentUpdatableEntityStatBase<int, DistrictCenter>
{
    public const string StatId = "Population";
    public override string Id => StatId;

    public bool TryGetTracker(PopulationCounterOptions options, UpdatableEntityStatComponent comp, [NotNullWhen(true)] out IEntityStatTracker<int>? tracker)
    {
        if (TryGetTracker(comp, out tracker))
        {
            ((DistrictPopulationStatTracker)tracker).Options = options;
            return true;
        }

        return false;
    }

    public GlobalPopulationStatTracker GetGlobalTracker(PopulationCounterOptions options)
        => new(options, service);

    protected override IEntityStatTracker<int> GetComponentTracker(UpdatableEntityStatComponent statComp, DistrictCenter comp)
        => new DistrictPopulationStatTracker(comp, statComp, service);
}

public class DistrictPopulationStatTracker(
    DistrictCenter dc,
    UpdatableEntityStatComponent comp,
    PopulationStatService service
) : StatTrackerBase<int>(comp)
{

    public PopulationCounterOptions Options { get; internal set; }

    protected override int CalculateValue()
        => service.GetData(dc, Options);

    protected override void OnPause()
    {
        service.OnPopulationChanged -= OnPopulationChanged;
    }

    protected override void OnStart()
    {
        service.OnPopulationChanged += OnPopulationChanged;
    }

    void OnPopulationChanged() => UpdateValue();
}

public class GlobalPopulationStatTracker(PopulationCounterOptions options, PopulationStatService service) : IStatTracker<int>
{
    public bool Running { get; private set; }
    public int Value { get; private set; }
    public string ValueFormatted => Value.ToString();

    public event EventHandler? OnValueChanged;
    public event EventHandler<int>? OnTypedValueChanged;

    public void Pause()
    {
        if (!Running) { return; }

        service.OnPopulationChanged -= OnPopulationChanged;
        Running = false;
    }

    void OnPopulationChanged() => UpdateValue();

    public void Start()
    {
        if (Running) { return; }

        Running = true;
        service.OnPopulationChanged += OnPopulationChanged;
        UpdateValue();
    }

    void UpdateValue(bool force = false)
    {
        var old = Value;
        Value = service.GetGlobalData(options);

        if (force || (old != Value && Running))
        {
            OnValueChanged?.Invoke(this, EventArgs.Empty);
            OnTypedValueChanged?.Invoke(this, Value);
        }
    }

    public void Dispose() => Pause();

    public void ForceUpdating() => UpdateValue(true);
}