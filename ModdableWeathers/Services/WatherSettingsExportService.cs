namespace ModdableWeathers.Services;

public class WatherSettingsExportService(
    ISystemFileDialogService fileDialogService,

    ModdableWeatherRegistry weatherRegistry,
    ModdableWeatherModifierRegistry modifierRegistry,
    
    ModdableWeatherSettingsService weatherSettings,
    ModdableWeatherModifierSettingsService modifierSettings,
    WeatherCycleStageDefinitionService weatherCycleStageDefinitionService
)
{
    const string WeathersKey = "Weathers";
    const string WeatherModifiersKey = "WeatherModifiers";
    const string WeatherCycleStagesKey = "WeatherCycleStages";

    public bool RequestExport()
    {
        var filePath = fileDialogService.ShowSaveFileDialog("json");
        if (filePath is null) { return false; }

        if (!filePath.EndsWith(".json", StringComparison.OrdinalIgnoreCase))
        {
            filePath += ".json";
        }

        var serialized = SerializeSettings().ToString();
        File.WriteAllText(filePath, serialized);

        return true;
    }

    public bool RequestImport()
    {
        var filePath = fileDialogService.ShowOpenFileDialog("json");
        if (filePath is null) { return false; }

        var fileContent = File.ReadAllText(filePath);
        var json = JObject.Parse(fileContent);

        weatherCycleStageDefinitionService.StagesDefinitions = json[WeatherCycleStagesKey]!
            .ToObject<ImmutableArray<WeatherCycleStageDefinition>>();

        weatherSettings.LoadSerializedSettings(json[WeathersKey]!.Value<JObject>()!);
        modifierSettings.LoadSerializedSettings(json[WeatherModifiersKey]!.Value<JObject>()!);

        weatherRegistry.ReloadSettings();
        modifierRegistry.ReloadSettings();

        return true;
    }

    JObject SerializeSettings() => new()
    {
        { WeathersKey, weatherSettings.SerializeSettings() },
        { WeatherModifiersKey, modifierSettings.SerializeSettings() },
        { WeatherCycleStagesKey, JArray.FromObject(weatherCycleStageDefinitionService.StagesDefinitions) },
    };

}
