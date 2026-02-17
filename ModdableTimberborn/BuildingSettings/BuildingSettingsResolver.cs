namespace ModdableTimberborn.BuildingSettings;

public class BuildingSettingsResolver(
    IEnumerable<IBuildingSettings> buildingSettings
)
{

    readonly FrozenDictionary<Type, IBuildingSettings> handlers
        = buildingSettings.ToFrozenDictionary(h => h.Type);
    public readonly ImmutableArray<IBuildingSettings> AllBuildingSettings
        = [.. buildingSettings.OrderBy(h => h.Order)];

    readonly Dictionary<string, ImmutableArray<IBuildingSettings>> templateCache = [];

    public IBuildingSettings? Get(Type type) => handlers.GetValueOrDefault(type);

    public BuildingSettingsPair? Get(IDuplicable duplicable)
    {
        var settings = Get(duplicable.GetType());
        return settings is null ? null : new BuildingSettingsPair(duplicable, settings);
    }

    public BuildingSettingsPair[] Get(BaseComponent comp) => Get(comp, null);

    public BuildingSettingsPair[] Get(BaseComponent comp, HashSet<IBuildingSettings>? filter)
    {
        var template = comp.GetTemplateName();
        IEnumerable<IBuildingSettings> settings = GetSettingsForObject(comp);

        if (filter is not null && filter.Count > 0)
        {
            settings = settings.Where(filter.Contains);
        }

        return [.. settings
            .Select(s => new BuildingSettingsPair(
                s.GetComponent(comp)!,
                s))];
    }

    public ImmutableArray<IBuildingSettings> GetSettingsForObject(BaseComponent comp)
    {
        return templateCache.GetOrAdd(comp.GetTemplateName(), GetComponentSettings);

        ImmutableArray<IBuildingSettings> GetComponentSettings()
        {
            List<IDuplicable> comps = [];
            comp.GetComponents(comps);

            return [.. comps
                .Select(c => Get(c.GetType()))
                .Where(s => s is not null)!];
        }
    }

}

public readonly record struct BuildingSettingsPair(IDuplicable Duplicable, IBuildingSettings Settings);