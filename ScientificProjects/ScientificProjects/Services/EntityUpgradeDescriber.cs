namespace ScientificProjects.Services;

public class EntityUpgradeDescriber(
    IEnumerable<ICharacterUpgradeDescriber> characterDescribers,
    IEnumerable<IWorkplaceUpgradeDescriber> workplaceDescribers,
    ScientificProjectService sp,
    ScientificProjectRegistry registry,
    ScientificProjectUnlockService unlocks
)
{
    public readonly ScientificProjectService ScientificProjectService = sp;
    public readonly ScientificProjectRegistry ScientificProjectRegistry = registry;
    public readonly ScientificProjectUnlockService ScientificProjectUnlockService = unlocks;

    public IEnumerable<EntityEffectDescription> DescribeEffects<T>(T comp, IEnumerable<IEntityUpgradeDescriber<T>> describers, ILoc t, IDayNightCycle dayNightCycle) where T : BaseComponent
    {
        var parameters = new DescribeEffectsParameters(
            ScientificProjectService.ActiveProjects,
            this,
            t,
            dayNightCycle
        );

        return describers
                .SelectMany(describer => describer
                    .DescribeEffects(comp, parameters));
    }

    public IEnumerable<EntityEffectDescription> DescribeEffects(CharacterProjectUpgradeComponent comp, ILoc t, IDayNightCycle dayNightCycle) 
        => DescribeEffects(comp, characterDescribers, t, dayNightCycle);

    public IEnumerable<EntityEffectDescription> DescribeEffects(WorkplaceProjectUpgradeComponent comp, ILoc t, IDayNightCycle dayNightCycle) 
        => DescribeEffects(comp, workplaceDescribers, t, dayNightCycle);
}