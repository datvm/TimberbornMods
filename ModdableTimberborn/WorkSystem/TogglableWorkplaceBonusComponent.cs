namespace ModdableTimberborn.WorkSystem;

public abstract class TogglableWorkplaceEffectComponent : BaseEventTogglableContainerComponent<WorkplaceTogglableContainer, Workplace, Worker>
{

    protected abstract void OnWorkerAssigned(Worker worker);
    protected abstract void OnWorkerUnassigned(Worker worker);

    protected override WorkplaceTogglableContainer CreateData(Workplace comp)
        => new(comp, OnWorkerAssigned, OnWorkerUnassigned);

}

public abstract class TogglableWorkplaceBonusComponent : TogglableWorkplaceEffectComponent
{

    protected abstract IReadOnlyList<BonusSpec> Bonuses { get; }

    protected override void OnWorkerAssigned(Worker worker) => worker.GetBonusManager().AddBonuses(Bonuses);
    protected override void OnWorkerUnassigned(Worker worker) => worker.GetBonusManager().RemoveBonuses(Bonuses);

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

    private void Container_WorkerAssigned(object sender, WorkerChangedEventArgs e) => OnMemberAdded(e.Worker);
    private void Container_WorkerUnassigned(object sender, WorkerChangedEventArgs e) => OnMemberRemoved(e.Worker);

    protected override void OnMemberAdded(Worker member) => onWorkerAssigned(member);
    protected override void OnMemberRemoved(Worker member) => onWorkerUnassigned(member);

}
