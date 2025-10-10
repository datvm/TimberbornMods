namespace ScientificProjects.Services;

public class EntityUpgradeDescriber(
    IEnumerable<ICharacterUpgradeDescriber> characterDescribers,
    IEnumerable<IWorkplaceUpgradeDescriber> workplaceDescribers,
    IEnumerable<IWorkplaceWorkerUpgradeDescriber> workplaceWorkerDescribers,
    ScientificProjectService sp,
    ScientificProjectRegistry registry,
    ScientificProjectUnlockService unlocks,
    ILoc t,
    IDayNightCycle dayNightCycle
)
{
    public readonly ScientificProjectService ScientificProjectService = sp;
    public readonly ScientificProjectRegistry ScientificProjectRegistry = registry;
    public readonly ScientificProjectUnlockService ScientificProjectUnlockService = unlocks;
    public readonly ILoc T = t;
    public readonly IDayNightCycle DayNightCycle = dayNightCycle;

    public IEnumerable<EntityEffectDescription> DescribeEffects<T>(T comp, IEnumerable<IEntityUpgradeDescriber<T>> describers) where T : BaseComponent
    {
        var p = GetParameters();

        return describers
                .SelectMany(describer => describer
                    .DescribeEffects(comp, p));
    }

    public IEnumerable<EntityEffectDescription> DescribeEffects(CharacterTrackerComponent comp) 
        => DescribeEffects(comp, characterDescribers);

    public IEnumerable<EntityEffectDescription> DescribeEffects(WorkplaceTrackerComponent comp) 
        => DescribeEffects(comp, workplaceDescribers);

    public IEnumerable<EntityEffectDescription> DescribeWorkplaceWorkerEffects(WorkplaceTrackerComponent comp, Worker worker)
    {
        var p = GetParameters();

        return workplaceWorkerDescribers
            .SelectMany(describer => describer
                .DescribeWorkerEffects(comp, worker, p));
    }

    DescribeEffectsParameters GetParameters() => new(
        ScientificProjectService.ActiveProjects,
        this
    );

}