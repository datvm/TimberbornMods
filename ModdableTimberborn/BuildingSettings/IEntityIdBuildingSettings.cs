namespace ModdableTimberborn.BuildingSettings;

public interface IEntityIdBuildingSettings
{
    bool Deserialize(string settings, IDuplicable target, IReadOnlyDictionary<Guid, Guid> idMappings);
}

public interface IEntityIdBuildingSettingsBase<T, TModel> : IEntityIdBuildingSettings, IBuildingSettings<T, TModel>
    where T : BaseComponent, IDuplicable<T>
    where TModel : IEntityIdModel
{
    bool Deserialize(string settings, T target, IReadOnlyDictionary<Guid, Guid> idMappings);
    bool IEntityIdBuildingSettings.Deserialize(string settings, IDuplicable target, IReadOnlyDictionary<Guid, Guid> idMappings)
        => Deserialize(settings, (T)target, idMappings);
}