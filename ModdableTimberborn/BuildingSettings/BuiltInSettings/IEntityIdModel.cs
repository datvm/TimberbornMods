namespace ModdableTimberborn.BuildingSettings.BuiltInSettings;

public interface IEntityIdModel
{
    Guid?[] EntityIds { get; set; }
}

public record EntityIdModelBase(Guid?[] EntityIds)
{
    public Guid?[] EntityIds { get; set; } = EntityIds;
}
