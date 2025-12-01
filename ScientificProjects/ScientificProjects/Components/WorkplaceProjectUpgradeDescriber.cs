namespace ScientificProjects.Components;

public class WorkplaceProjectUpgradeDescriber : BaseComponent, IWorkplaceEntityMultiEffectsDescriber, IAwakableComponent
{
    
#nullable disable
    EntityUpgradeDescriber entityUpgradeDescriber;
    WorkplaceTrackerComponent workplaceTrackerComponent;
#nullable enable

    [Inject]
    public void Inject(EntityUpgradeDescriber entityUpgradeDescriber)
    {
        this.entityUpgradeDescriber = entityUpgradeDescriber;
    }

    public void Awake()
    {
        workplaceTrackerComponent = GetComponent<WorkplaceTrackerComponent>();
    }

    public IEnumerable<EntityEffectDescription> DescribeAll(ILoc t, IDayNightCycle dayNightCycle)
        => entityUpgradeDescriber.DescribeEffects(workplaceTrackerComponent);

    public IEnumerable<EntityEffectDescription> DescribeAllWorkerEffects(Worker worker, ILoc t, IDayNightCycle dayNightCycle)
        => entityUpgradeDescriber.DescribeWorkplaceWorkerEffects(workplaceTrackerComponent, worker);
}
