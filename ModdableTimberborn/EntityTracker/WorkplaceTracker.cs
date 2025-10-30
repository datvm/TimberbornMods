namespace ModdableTimberborn.EntityTracker;

public class WorkplaceTracker : IEntityTracker<WorkplaceTrackerComponent>
{
    readonly HashSet<WorkplaceTrackerComponent> entities = [];
    public IReadOnlyCollection<WorkplaceTrackerComponent> Entities => entities;

    public IEnumerable<WorkplaceTrackerComponent> BuilderWorkplaces => entities.Where(e => e.IsBuilderWorkplace);

    public event Action<WorkplaceTrackerComponent>? OnEntityRegistered;
    public event Action<WorkplaceTrackerComponent>? OnEntityUnregistered;

    public event Action<WorkplaceTrackerComponent, Worker>? OnWorkerAssigned;
    public event Action<WorkplaceTrackerComponent, Worker>? OnWorkerUnassigned;

    public void Track(EntityComponent entity)
    {
        var comp = entity.GetComponent<WorkplaceTrackerComponent>();
        if (!comp) { return; }

        entities.Add(comp);

        var wp = comp.Workplace;
        wp.WorkerAssigned += (_, e) => OnWorkerAssigned?.Invoke(comp, e.Worker);
        wp.WorkerUnassigned += (_, e) => OnWorkerUnassigned?.Invoke(comp, e.Worker);

        OnEntityRegistered?.Invoke(comp);
    }

    public void Untrack(EntityComponent entity)
    {
        var comp = entity.GetComponent<WorkplaceTrackerComponent>();
        if (!comp) { return; }

        entities.Remove(comp);
        OnEntityUnregistered?.Invoke(comp);
    }
}
