namespace ModdableTimberborn.WorkSystem;

public abstract class TogglableWorkplaceEffectComponent : BaseEventTogglableContainerComponent<WorkplaceTogglableContainer, Workplace, Worker>
{

    protected abstract void OnWorkerAssigned(Worker worker);
    protected abstract void OnWorkerUnassigned(Worker worker);

    protected override WorkplaceTogglableContainer CreateData(Workplace comp)
        => new(comp, OnWorkerAssigned, OnWorkerUnassigned);

}

/// <summary>
/// A togglable component that applies/removes bonuses to/from workers when they are assigned/unassigned to/from the workplace.
/// Must have <see cref="ModdableTimberbornRegistry.UseBonusTracker(bool)"/> registered.
/// </summary>
public abstract class TogglableWorkplaceBonusComponent : TogglableWorkplaceEffectComponent
{

    protected abstract BonusTrackerItem Bonuses { get; }

    protected override void OnWorkerAssigned(Worker worker) => worker.GetBonusTracker().AddOrUpdate(Bonuses);
    protected override void OnWorkerUnassigned(Worker worker) => worker.GetBonusTracker().Remove(Bonuses.Id);

}

public class WorkplaceTogglableContainer(Workplace workplace, Action<Worker> onWorkerAssigned, Action<Worker> onWorkerUnassigned) : BaseEventTogglableContainer<Workplace, Worker>
{

    public override Workplace Container { get; } = workplace;
    public override IEnumerable<Worker> Members => Container.AssignedWorkers;

    protected override void AddEventHandlers()
    {
        Container.WorkerAssigned += Container_WorkerAssigned;
        Container.WorkerUnassigned += Container_WorkerUnassigned;
    }

    protected override void RemoveEventHandlers()
    {
        Container.WorkerAssigned -= Container_WorkerAssigned;
        Container.WorkerUnassigned -= Container_WorkerUnassigned;
    }

    void Container_WorkerAssigned(object sender, WorkerChangedEventArgs e) => OnMemberAdded(e.Worker);
    void Container_WorkerUnassigned(object sender, WorkerChangedEventArgs e) => OnMemberRemoved(e.Worker);

    protected override void OnMemberAdded(Worker member) => onWorkerAssigned(member);
    protected override void OnMemberRemoved(Worker member) => onWorkerUnassigned(member);

}
