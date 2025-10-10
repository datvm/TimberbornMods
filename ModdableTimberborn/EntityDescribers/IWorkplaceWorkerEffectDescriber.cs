namespace ModdableTimberborn.EntityDescribers;

public interface IWorkplaceWorkerEffectDescriber : IEntityEffectDescriber
{
    EntityEffectDescription? DescribeWorkerEffect(Worker worker, ILoc t, IDayNightCycle dayNightCycle);
}

public interface IWorkplaceEntityMultiEffectsDescriber : IEntityMultiEffectsDescriber
{
    IEnumerable<EntityEffectDescription> DescribeAllWorkerEffects(Worker worker, ILoc t, IDayNightCycle dayNightCycle);
}