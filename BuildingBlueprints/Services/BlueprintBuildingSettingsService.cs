namespace BuildingBlueprints.Services;

[BindSingleton]
public class BlueprintBuildingSettingsService(BuildingSettingsResolver buildingSettingsResolver)
{

    public JObject GetSettings(BaseComponent building)
    {
        var settings = buildingSettingsResolver.Get(building);
        JObject result = [];

        foreach (var (d, s) in settings)
        {
            var serialized = s.Serialize(d);
            var jObj = JObject.Parse(serialized);

            result.Add(s.Type.FullName, jObj);
        }

        return result;
    }

    public void ApplySettings(BaseComponent building, JObject? settings, Dictionary<Guid, Guid> idMapping)
    {
        if (settings is null || !building) { return; }

        var buildingSettings = buildingSettingsResolver.Get(building);
        foreach (var (d, s) in buildingSettings)
        {
            try
            {
                var name = s.Type.FullName;
                if (!settings.TryGetValue(name, out var jValue)) { continue; }

                var serialized = jValue.ToString(Formatting.None);

                switch (s)
                {
                    case IEntityIdBuildingSettings idSettings:
                        idSettings.Deserialize(serialized, d, idMapping);
                        break;
                    default:
                        s.Deserialize(serialized, d);
                        break;
                }
            }
            catch (Exception ex)
            {
                Debug.LogError($"Failed to apply building settings for {building.Name} ({building.GetType().FullName}): {ex}");
            }
            
        }
    }

}
