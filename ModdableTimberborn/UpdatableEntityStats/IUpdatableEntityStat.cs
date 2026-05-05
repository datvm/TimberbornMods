namespace ModdableTimberborn.UpdatableEntityStats;

public interface IUpdatableEntityStat
{
    string Id { get; }
    string DisplayLoc { get; }

    Type StatType { get; }
    bool TryGetTracker(UpdatableEntityStatComponent? comp, [NotNullWhen(true)] out IEntityStatTracker? tracker);
    bool CanTrack(UpdatableEntityStatComponent? comp);
}

public interface IUpdatableEntityStat<T> : IUpdatableEntityStat
{
    Type IUpdatableEntityStat.StatType => typeof(T);
    bool IUpdatableEntityStat.TryGetTracker(UpdatableEntityStatComponent? comp, [NotNullWhen(true)] out IEntityStatTracker? tracker)
    {
        if (TryGetTracker(comp, out var typedTracker))
        {
            tracker = typedTracker;
            return true;
        }

        tracker = null;
        return false;
    }

    bool TryGetTracker(UpdatableEntityStatComponent? comp, [NotNullWhen(true)] out IEntityStatTracker<T>? tracker);
}

public interface IImageStat : IUpdatableEntityStat<Sprite?> { }

public interface IPercentStat : IUpdatableEntityStat<float>
{
    bool TryGetTracker(UpdatableEntityStatComponent? comp, [NotNullWhen(true)] out IEntityPercentStatTracker? tracker);

    bool IUpdatableEntityStat.TryGetTracker(UpdatableEntityStatComponent? comp, [NotNullWhen(true)] out IEntityStatTracker? tracker)
    {
        if (TryGetTracker(comp, out var typedTracker))
        {
            tracker = typedTracker;
            return true;
        }

        tracker = null;
        return false;
    }

}