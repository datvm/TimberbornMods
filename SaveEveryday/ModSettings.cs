namespace SaveEveryday;

public class ModSettings(ISettings settings, ModSettingsOwnerRegistry modSettingsOwnerRegistry, ModRepository modRepository) : ModSettingsOwner(settings, modSettingsOwnerRegistry, modRepository)
{

    public override string ModId => nameof(SaveEveryday);
    public override ModSettingsContext ChangeableOn => ModSettingsContext.All;

    readonly ModSetting<bool> enabled = new(
        true,
        ModSettingDescriptor
            .CreateLocalized("LV.SE.Enabled")
            .SetLocalizedTooltip("LV.SE.EnabledDesc"));

    readonly RangeIntModSetting saveFrequency = new(
        1, 1, 10,
        ModSettingDescriptor
            .CreateLocalized("LV.SE.SaveFrequency")
            .SetLocalizedTooltip("LV.SE.SaveFrequencyDesc"));

    readonly RangeIntModSetting saveCount = new(
        5, 1, 20,
        ModSettingDescriptor
            .CreateLocalized("LV.SE.SaveCount")
            .SetLocalizedTooltip("LV.SE.SaveCountDesc"));

    readonly ModSetting<bool> saveWeatherWarning = new(
        false,
        ModSettingDescriptor
            .CreateLocalized("LV.SE.SaveWeatherWarning")
            .SetLocalizedTooltip("LV.SE.SaveWeatherWarningDesc"));

    readonly ModSetting<bool> autosaveWarning = new(
        false,
        ModSettingDescriptor
            .CreateLocalized("LV.SE.AutosaveWarning")
            .SetLocalizedTooltip("LV.SE.AutosaveWarningDesc"));

    public bool Enabled { get; private set; } = false;
    public int SaveFrequency { get; private set; } = 1;
    public int SaveCount { get; private set; } = 3;
    public bool SaveWeatherWarning { get; private set; } = false;
    public bool AutosaveWarning { get; private set; } = false;

    public override void OnAfterLoad()
    {
        saveFrequency.Descriptor.SetEnableCondition(() => enabled.Value);
        saveCount.Descriptor.SetEnableCondition(() => enabled.Value);
        saveWeatherWarning.Descriptor.SetEnableCondition(() => enabled.Value);
        autosaveWarning.Descriptor.SetEnableCondition(() => enabled.Value);

        AddCustomModSetting(enabled, nameof(enabled));
        AddCustomModSetting(saveFrequency, nameof(saveFrequency));
        AddCustomModSetting(saveCount, nameof(saveCount));
        AddCustomModSetting(autosaveWarning, nameof(autosaveWarning));
        AddCustomModSetting(saveWeatherWarning, nameof(saveWeatherWarning));

        ModSettingChanged += (_, _) => UpdateValues();
        UpdateValues();
    }

    void UpdateValues()
    {
        Enabled = enabled.Value;
        SaveFrequency = saveFrequency.Value;
        SaveCount = saveCount.Value;
        SaveWeatherWarning = saveWeatherWarning.Value;
        AutosaveWarning = autosaveWarning.Value;
    }

}
