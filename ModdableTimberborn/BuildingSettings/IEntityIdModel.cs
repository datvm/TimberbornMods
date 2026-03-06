namespace ModdableTimberborn.BuildingSettings;

public interface IEntityIdModel
{
    Guid?[] EntityIds { get; set; }
}

public record EntityIdModelBase(Guid?[] EntityIds) : IEntityIdModel
{
    public Guid?[] EntityIds { get; set; } = EntityIds;
}
