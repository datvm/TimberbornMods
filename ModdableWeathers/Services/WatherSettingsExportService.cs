namespace ModdableWeathers.Services;

public class WatherSettingsExportService(
    ISystemFileDialogService fileDialogService,

    ModdableWeatherRegistry weatherRegistry,
    ModdableWeatherModifierRegistry modifierRegistry,

    ModdableWeatherSettingsService weatherSettings,
    ModdableWeatherModifierSettingsService modifierSettings,
    WeatherCycleStageDefinitionService weatherCycleStageDefinitionService,
    GeneralWeatherSettings generalWeatherSettings
)
{
    readonly IBaseWeatherSettings generalWeatherSettings = generalWeatherSettings;

    const string WeathersKey = "Weathers";
    const string WeatherModifiersKey = "WeatherModifiers";
    const string WeatherCycleStagesKey = "WeatherCycleStages";
    const string GeneralWeatherSettingsKey = "GeneralWeatherSettings";

    public bool RequestExport()
    {
        var filePath = fileDialogService.ShowSaveFileDialog("json");
        if (filePath is null) { return false; }

        if (!filePath.EndsWith(".json", StringComparison.OrdinalIgnoreCase))
        {
            filePath += ".json";
        }

        ExportToPath(filePath);

        return true;
    }

    public void ExportToPath(string filePath)
    {
        var serialized = SerializeSettings().ToString();
        File.WriteAllText(filePath, serialized);
    }

    public bool RequestImport()
    {
        var filePath = fileDialogService.ShowOpenFileDialog("json");
        if (filePath is null) { return false; }

        ImportFromPath(filePath);
        return true;
    }

    public void ImportFromPath(string filePath)
    {
        var fileContent = File.ReadAllText(filePath);
        var json = JObject.Parse(fileContent);


        var cycleDef = json[WeatherCycleStagesKey]?.ToObject<ImmutableArray<WeatherCycleStageDefinition>>();
        if (cycleDef is not null && cycleDef.Value != default)
        {
            weatherCycleStageDefinitionService.StagesDefinitions = cycleDef.Value;
        }

        LoadIfHasValue(GeneralWeatherSettingsKey, generalWeatherSettings.Deserialize);

        LoadIfHasValue(WeathersKey, weatherSettings.LoadSerializedSettings);
        LoadIfHasValue(WeatherModifiersKey, modifierSettings.LoadSerializedSettings);

        weatherRegistry.ReloadSettings();
        modifierRegistry.ReloadSettings();

        void LoadIfHasValue(string name, Action<JObject> action)
        {
            var value = json[name]?.Value<JObject>();
            if (value is not null) { action(value); }
        }
    }

    JObject SerializeSettings() => new()
    {
        { GeneralWeatherSettingsKey, generalWeatherSettings.Serialize() },
        { WeatherCycleStagesKey, JArray.FromObject(weatherCycleStageDefinitionService.StagesDefinitions) },
        { WeathersKey, weatherSettings.SerializeSettings() },
        { WeatherModifiersKey, modifierSettings.SerializeSettings() },
    };

}
