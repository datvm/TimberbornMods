namespace ModdableTimberborn.UpdatableEntityStats;

public abstract class StatPercentTrackerBase(UpdatableEntityStatComponent comp) : StatTrackerBase<float>(comp), IPercentStatTracker
{
    public override string ValueFormatted => Value.ToString("P0");
}

public abstract class StatTrackerBase<T> : IEntityStatTracker<T>
{
    protected readonly UpdatableEntityStatComponent comp;

    public event EventHandler? OnValueChanged;
    public event EventHandler<T?>? OnTypedValueChanged;
    public event EventHandler<UpdatableEntityStatComponent>? OnEntityLost;

    public UpdatableEntityStatComponent Component => comp;

    public bool Running { get; private set; }
    public T? Value { get; protected set; }
    public virtual string ValueFormatted => Value?.ToString() ?? "?";

    public StatTrackerBase(UpdatableEntityStatComponent comp)
    {
        this.comp = comp;
        comp.AddTracker(this);
    }

    public virtual void Start()
    {
        if (Running) { return; }

        Running = true;
        OnStart();
        UpdateValue();
    }

    public virtual void Pause()
    {
        if (!Running) { return; }

        Running = false;
        OnPause();
    }

    protected abstract void OnStart();
    protected abstract void OnPause();

    protected abstract T CalculateValue();

    public void ForceUpdating() => UpdateValue(true);

    protected void UpdateValue(bool force = false)
    {
        var oldValue = Value;
        Value = CalculateValue();

        if (force || (!EqualityComparer<T?>.Default.Equals(oldValue, Value) && Running))
        {
            OnValueChanged?.Invoke(this, EventArgs.Empty);
            OnTypedValueChanged?.Invoke(this, Value);
        }
    }

    public void NotifyEntityLost() => OnEntityLost?.Invoke(this, comp);

    public void Dispose()
    {
        Pause();
        comp.RemoveTracker(this);
    }

}