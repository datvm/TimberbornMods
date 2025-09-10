namespace ModdableTimberborn.EntityDescribers;

public interface IBaseEntityDescriber
{
    int Order { get; }
}

public interface IEntityDescriber : IBaseEntityDescriber
{
    EntityDescription? Describe(ILoc t, IDayNightCycle dayNightCycle);
}

public interface IEntityMultiDescriber : IBaseEntityDescriber
{
    IEnumerable<EntityDescription> DescribeAll(ILoc t, IDayNightCycle dayNightCycle);
}

public readonly record struct EntityDescription(string Title, string Description, float? RemainingHours = null);