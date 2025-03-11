namespace ConfigurableAutoSave;

public class MSettings(ISettings settings, ModSettingsOwnerRegistry modSettingsOwnerRegistry, ModRepository modRepository) : ModSettingsOwner(settings, modSettingsOwnerRegistry, modRepository)
{
    readonly ModSetting<bool> enabled = new(
        true,
        ModSettingDescriptor
            .CreateLocalized("LV.CAS.Enabled")
            .SetLocalizedTooltip("LV.CAS.EnabledDesc"));

    readonly ModSetting<float> saveFrequency = new(
        5,
        ModSettingDescriptor
            .CreateLocalized("LV.CAS.SaveFrequency")
            .SetLocalizedTooltip("LV.CAS.SaveFrequencyDesc"));

    readonly RangeIntModSetting saveCount = new(
        5, 1, 20,
        ModSettingDescriptor
            .CreateLocalized("LV.CAS.SaveCount")
            .SetLocalizedTooltip("LV.CAS.SaveCountDesc"));

    readonly ModSetting<bool> saveWeatherWarning = new(
        false,
        ModSettingDescriptor
            .CreateLocalized("LV.CAS.SaveWeatherWarning")
            .SetLocalizedTooltip("LV.CAS.SaveWeatherWarningDesc"));

    public event Action OnSettingsChanged = delegate { };

    public bool Enabled { get; private set; } = false;
    public float SaveFrequency { get; private set; } = 5f;
    public int SaveCount { get; private set; } = 5;
    public bool SaveWeatherWarning { get; private set; } = false;

    public override string ModId => nameof(ConfigurableAutoSave);
    public override ModSettingsContext ChangeableOn => ModSettingsContext.All;

    public override void OnAfterLoad()
    {
        base.OnAfterLoad();

        saveFrequency.Descriptor.SetEnableCondition(() => enabled.Value);
        saveCount.Descriptor.SetEnableCondition(() => enabled.Value);

        AddCustomModSetting(enabled, nameof(enabled));
        AddCustomModSetting(saveFrequency, nameof(saveFrequency));
        AddCustomModSetting(saveCount, nameof(saveCount));
        AddCustomModSetting(saveWeatherWarning, nameof(saveWeatherWarning));

        enabled.ValueChanged += (_, _) => UpdateValues();
        saveFrequency.ValueChanged += (_, _) => UpdateValues();
        saveCount.ValueChanged += (_, _) => UpdateValues();
        saveWeatherWarning.ValueChanged += (_, _) => UpdateValues();

        UpdateValues();
    }

    void UpdateValues()
    {
        Enabled = enabled.Value;
        SaveFrequency = saveFrequency.Value;
        SaveCount = saveCount.Value;
        SaveWeatherWarning = saveWeatherWarning.Value;

        OnSettingsChanged();
    }
}
