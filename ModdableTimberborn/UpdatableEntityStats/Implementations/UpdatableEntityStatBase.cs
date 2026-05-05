namespace ModdableTimberborn.UpdatableEntityStats;

public abstract class UpdatableEntityStatBase<T> : IUpdatableEntityStat<T>
{
    public abstract string Id { get; }
    public virtual string DisplayLoc => "LV.MT.UStat." + Id;
    public Type StatType => typeof(T);

    public abstract bool CanTrack(UpdatableEntityStatComponent? comp);
    public abstract bool TryGetTracker(UpdatableEntityStatComponent? comp, [NotNullWhen(true)] out IEntityStatTracker<T>? tracker);
}

public abstract class ComponentUpdatableEntityStatBase<T, TComp> : UpdatableEntityStatBase<T>
    where TComp : BaseComponent
{

    public override bool CanTrack(UpdatableEntityStatComponent? comp) 
        => IsComponentValid(comp?.GetComponent<TComp>());

    protected virtual bool IsComponentValid([NotNullWhen(true)] TComp? component) => component;

    public override bool TryGetTracker(UpdatableEntityStatComponent? comp, [NotNullWhen(true)] out IEntityStatTracker<T>? tracker)
    {
        tracker = null;

        var c = comp?.GetComponent<TComp>();
        if (!IsComponentValid(c)) { return false; }

        tracker = GetComponentTracker(comp!, c);
        return tracker is not null;
    }

    protected abstract IEntityStatTracker<T>? GetComponentTracker(UpdatableEntityStatComponent statComp, TComp comp);

}

public abstract class ComponentUpdatableEntityPercentStatBase<TComp> : ComponentUpdatableEntityStatBase<float, TComp>, IPercentStat
    where TComp : BaseComponent
{

    public override bool TryGetTracker(UpdatableEntityStatComponent? comp, [NotNullWhen(true)] out IEntityStatTracker<float>? tracker)
    {
        if (TryGetTracker(comp, out IEntityPercentStatTracker? percentTracker))
        {
            tracker = percentTracker;
            return true;
        }

        tracker = null;
        return false;
    }

    public bool TryGetTracker(UpdatableEntityStatComponent? comp, [NotNullWhen(true)] out IEntityPercentStatTracker? tracker)
    {
        tracker = null;

        var c = comp?.GetComponent<TComp>();
        if (!IsComponentValid(c)) { return false; }

        tracker = GetComponentPercentTracker(comp!, c);
        return tracker is not null;
    }

    protected override IEntityStatTracker<float>? GetComponentTracker(UpdatableEntityStatComponent statComp, TComp comp)
        => GetComponentPercentTracker(statComp, comp);

    protected abstract IEntityPercentStatTracker? GetComponentPercentTracker(UpdatableEntityStatComponent statComp, TComp comp);

}