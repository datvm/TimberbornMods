namespace ModdableTimberborn.UpdatableEntityStats;

public class UpdatableEntityStatComponent : BaseComponent, IDeletableEntity
{

    readonly HashSet<IEntityStatTracker> trackers = [];
    public IReadOnlyCollection<IEntityStatTracker> Trackers => trackers;

    public void AddTracker(IEntityStatTracker tracker)
    {
        trackers.Add(tracker);
    }

    public void RemoveTracker(IEntityStatTracker tracker)
    {
        trackers.Remove(tracker);
    }

    public void ClearTrackers()
    {
        foreach (var t in trackers.ToArray())
        {
            t.Dispose();
        }
        trackers.Clear();
    }

    public void DeleteEntity()
    {
        if (trackers.Count == 0) { return; }

        foreach (var t in trackers.ToArray())
        {
            t.NotifyEntityLost();
        }

        ClearTrackers();
    }
}
