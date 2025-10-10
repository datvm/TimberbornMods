namespace ScientificProjects.Services;

public interface IWorkplaceUpgradeDescriber : IEntityUpgradeDescriber<WorkplaceTrackerComponent> { }

public interface IWorkplaceWorkerUpgradeDescriber : IWorkplaceUpgradeDescriber
{

    IEnumerable<EntityEffectDescription> DescribeWorkerEffects(WorkplaceTrackerComponent workplace, Worker worker, DescribeEffectsParameters parameters);


}