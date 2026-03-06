namespace ModdableTimberborn.BuildingSettings;

public abstract class EntityIdBuildingSettingsBase<T, TModel>(ILoc t) : BuildingSettingsBase<T, TModel>(t), IEntityIdBuildingSettingsBase<T, TModel>
    where T : BaseComponent, IDuplicable<T>
    where TModel : IEntityIdModel
{

    public bool Deserialize(string settings, T target, IReadOnlyDictionary<Guid, Guid> idMappings) =>
        DeserializeInternal(settings, target, model =>
        {
            var ids = model.EntityIds;
            for (int i = 0; i < ids.Length; i++)
            {
                var original = ids[i];

                if (original.HasValue && idMappings.TryGetValue(original.Value, out var mappedId))
                {
                    ids[i] = mappedId;
                }
            }

            return model;
        });
}
