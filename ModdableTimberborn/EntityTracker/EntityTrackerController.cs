namespace ModdableTimberborn.EntityTracker;

public class EntityTrackerController(
    EventBus eb,
    IEnumerable<IEntityTracker> trackers
) : ILoadableSingleton
{

    public virtual void Load()
    {
        eb.Register(this);
    }

    [OnEvent]
    public virtual void OnEntityInitialized(EntityInitializedEvent e)
    {
        var entity = e.Entity;

        foreach (var tracker in trackers)
        {
            tracker.Track(entity);
        }
    }

    [OnEvent]
    public virtual void OnEntityDeleted(EntityDeletedEvent e)
    {
        var entity = e.Entity;

        foreach (var tracker in trackers)
        {
            tracker.Untrack(entity);
        }
    }

}
