namespace ModdableWeather.Settings;

public class SingleWeatherModeSettings(ISettings settings, ModSettingsOwnerRegistry modSettingsOwnerRegistry, ModRepository modRepository) 
    : ModSettingsOwner(settings, modSettingsOwnerRegistry, modRepository),
    IExportableSettings
{
    public string WeatherId { get; } = nameof(SingleWeatherModeSettings);

    public override string ModId { get; } = nameof(ModdableWeather);
    public override int Order { get; } = -1;
    public override string HeaderLocKey { get; } = "LV.MW.SingleWeatherMode";

    public ModSetting<bool> Enabled { get; } = new(false, ModSettingDescriptor
        .CreateLocalized("LV.MW.SWEnable")
        .SetLocalizedTooltip("LV.MW.SWEnableDesc"));

    public ModSetting<int> StartCycle { get; } = new(5, ModSettingDescriptor
        .CreateLocalized("LV.MW.SWStartCycle")
        .SetLocalizedTooltip("LV.MW.SWStartCycleDesc"));

    public RangeIntModSetting Chance { get; } = new(30, 0, 100, ModSettingDescriptor
        .CreateLocalized("LV.MW.SWChance")
        .SetLocalizedTooltip("LV.MW.SWChanceDesc"));

    public RangeIntModSetting TemperateChance { get; } = new(50, 0, 100, ModSettingDescriptor
        .CreateLocalized("LV.MW.SWTemperateChance")
        .SetLocalizedTooltip("LV.MW.SWTemperateChanceDesc"));

    public override void OnBeforeLoad()
    {
        ModSetting[] settings = [StartCycle, Chance, TemperateChance];
        foreach (var s in settings)
        {
            s.Descriptor.SetEnableCondition(() => Enabled.Value);
        }
    }

    public string Export()
    {
        return JsonConvert.SerializeObject(new SingleWeatherModeSettingsParameters(
            Enabled.Value,
            StartCycle.Value,
            Chance.Value,
            TemperateChance.Value
        ));
    }

    public void Import(string value)
    {
        var parameters = JsonConvert.DeserializeObject<SingleWeatherModeSettingsParameters>(value);
        
        Enabled.Value = parameters.Enabled;
        StartCycle.Value = parameters.StartCycle;
        Chance.Value = parameters.Chance;
        TemperateChance.Value = parameters.TemperateChance;
    }

}

public readonly record struct SingleWeatherModeSettingsParameters(bool Enabled, int StartCycle, int Chance, int TemperateChance);