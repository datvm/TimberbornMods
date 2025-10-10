namespace ScientificProjects.Services;

public class WorkplaceTracker
{
    public event Action<WorkplaceProjectUpgradeComponent>? OnRegistered;
    public event Action<WorkplaceProjectUpgradeComponent>? OnUnregistered;
    
    public event Action<WorkplaceProjectUpgradeComponent, Worker>? OnWorkerAssigned;
    public event Action<WorkplaceProjectUpgradeComponent, Worker>? OnWorkerUnassigned;

    readonly HashSet<WorkplaceProjectUpgradeComponent> workplaces = [];
    public IReadOnlyCollection<WorkplaceProjectUpgradeComponent> Workplaces => workplaces;

    public IEnumerable<WorkplaceProjectUpgradeComponent> BuilderWorkplaces => workplaces.Where(wp => wp.IsBuilderWorkplace);

    public void Register(WorkplaceProjectUpgradeComponent comp)
    {
        workplaces.Add(comp);

        var wp = comp.Workplace;
        wp.WorkerAssigned += (_, e) => InternalOnWorkerAssigned(comp, e.Worker);
        wp.WorkerUnassigned += (_, e) => InternalOnWorkerUnassigned(comp, e.Worker);

        OnRegistered?.Invoke(comp);
    }

    void InternalOnWorkerAssigned(WorkplaceProjectUpgradeComponent workplace, Worker worker)
    {
        OnWorkerAssigned?.Invoke(workplace, worker);
    }

    void InternalOnWorkerUnassigned(WorkplaceProjectUpgradeComponent workplace, Worker worker)
    {
        OnWorkerUnassigned?.Invoke(workplace, worker);
    }

    public void Unregister(WorkplaceProjectUpgradeComponent comp)
    {
        workplaces.Remove(comp);
        OnUnregistered?.Invoke(comp);
    }

}
