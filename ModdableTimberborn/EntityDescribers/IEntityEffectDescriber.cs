namespace ModdableTimberborn.EntityDescribers;

public interface IBaseEntityEffectDescriber
{
    int Order { get; }
}

public interface IEntityEffectDescriber : IBaseEntityEffectDescriber
{
    int IBaseEntityEffectDescriber.Order => 0;

    EntityEffectDescription? Describe(ILoc t, IDayNightCycle dayNightCycle);
}

public interface IEntityMultiEffectsDescriber : IBaseEntityEffectDescriber
{
    int IBaseEntityEffectDescriber.Order => 0;

    IEnumerable<EntityEffectDescription> DescribeAll(ILoc t, IDayNightCycle dayNightCycle);
}

public readonly record struct EntityEffectDescription(string Title, string Description, float? RemainingHours = null);