namespace TimberLive.Services;

[SelfService(Lifetime = ServiceLifetime.Singleton)]
public class AutomationBuildingRegistry
{

    public readonly ImmutableArray<IClientAutomationBuilding> Buildings;
    public readonly FrozenDictionary<string, IClientAutomationBuilding> BuildingsBySettings;

    public AutomationBuildingRegistry(IEnumerable<IClientAutomationBuilding> buildings)
    {
        Buildings = [.. buildings];
        BuildingsBySettings = Buildings.ToFrozenDictionary(b => b.SettingsName, b => b);
    }

    public bool FindAutomationBuilding(HttpBuilding building, [NotNullWhen(true)] out (IClientAutomationBuilding, object)? automationBuilding)
    {
        foreach (var (k, v) in building.Settings)
        {
            if (BuildingsBySettings.TryGetValue(k, out var info))
            {
                var settings = JsonSerializer.Deserialize(v, info.SettingsType)
                    ?? throw new JsonException($"Failed to deserialize settings for {info.DisplayTypeName}");
                automationBuilding = (info, settings);
                return true;
            }
        }

        automationBuilding = null;
        return false;
    }

}
