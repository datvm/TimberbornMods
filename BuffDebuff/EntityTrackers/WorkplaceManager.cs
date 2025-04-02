namespace BuffDebuff;

public class WorkplaceWorkerChangedEvent(Workplace workplace, Worker worker)
{
    public Workplace Workplace { get; } = workplace;
    public Worker Worker { get; } = worker;
}
public class WorkplaceWorkerAssignedEvent(Workplace workplace, Worker worker) : WorkplaceWorkerChangedEvent(workplace, worker);
public class WorkplaceWorkerUnassignedEvent(Workplace workplace, Worker worker) : WorkplaceWorkerChangedEvent(workplace, worker);

public class WorkplaceManager(EventBus eb, EntityManager entities) : ILoadableSingleton
{
    readonly HashSet<Workplace> activeWorkplaces = [];

    public ReadOnlyHashSet<Workplace> AllWorkplaces => entities.Get<Workplace>();
    public ReadOnlyHashSet<Workplace> ActiveWorkplaces => activeWorkplaces.AsReadOnly();

    public void Load()
    {
        eb.Register(this);
    }

    [OnEvent]
    public void OnEnteredFinishedState(EnteredFinishedStateEvent ev)
    {
        var wp = ev.BlockObject.GetComponentFast<Workplace>();
        if (wp is null) { return; }

        wp.WorkerAssigned += Wp_WorkerAssigned;
        wp.WorkerUnassigned += Wp_WorkerUnassigned;

        activeWorkplaces.Add(wp);
    }

    [OnEvent]
    public void OnExitedFinishedState(ExitedFinishedStateEvent ev)
    {
        var wp = ev.BlockObject.GetComponentFast<Workplace>();
        if (wp is null) { return; }

        wp.WorkerAssigned -= Wp_WorkerAssigned;
        wp.WorkerUnassigned -= Wp_WorkerUnassigned;

        activeWorkplaces.Remove(wp);
    }

    private void Wp_WorkerUnassigned(object sender, WorkerChangedEventArgs e)
    {
        // Don't use e.Worker.Workplace because it may be null already
        var workplace = (Workplace)sender;
        
        eb.Post(new WorkplaceWorkerUnassignedEvent(workplace, e.Worker));
        eb.Post(new WorkplaceWorkerChangedEvent(workplace, e.Worker));
    }

    private void Wp_WorkerAssigned(object sender, WorkerChangedEventArgs e)
    {
        // Don't use e.Worker.Workplace because it may still be null
        var workplace = (Workplace)sender;

        eb.Post(new WorkplaceWorkerAssignedEvent(workplace, e.Worker));
        eb.Post(new WorkplaceWorkerChangedEvent(workplace, e.Worker));
    }

}
