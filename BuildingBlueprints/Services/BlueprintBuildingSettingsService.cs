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

    public void ApplySettings(BaseComponent building, JObject? settings)
    {
        if (settings is null || !building) { return; }

        var buildingSettings = buildingSettingsResolver.Get(building);
        foreach (var (d, s) in buildingSettings)
        {
            var name = s.Type.FullName;
            if (!settings.TryGetValue(name, out var jValue)) { continue; }

            var serialized = jValue.ToString(Formatting.None);
            s.Deserialize(serialized, d);
        }
    }

}
