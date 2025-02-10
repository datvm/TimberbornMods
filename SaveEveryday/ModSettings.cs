namespace SaveEveryday;

public class ModSettings(ISettings settings, ModSettingsOwnerRegistry modSettingsOwnerRegistry, ModRepository modRepository) : ModSettingsOwner(settings, modSettingsOwnerRegistry, modRepository)
{

    public override string ModId => nameof(SaveEveryday);
    public override ModSettingsContext ChangeableOn => ModSettingsContext.All;

    public ModSetting<bool>? enabled;
    public RangeIntModSetting? saveFrequency, saveCount;

    public bool Enabled { get; private set; } = false;
    public int SaveFrequency { get; private set; } = 1;
    public int SaveCount { get; private set; } = 3;

    public override void OnAfterLoad()
    {
        enabled = new ModSetting<bool>(
            true,
            ModSettingDescriptor
                .CreateLocalized("LV.SE.Enabled")
                .SetLocalizedTooltip("LV.SE.EnabledDesc"));

        saveFrequency = new RangeIntModSetting(
            1, 1, 10,
            ModSettingDescriptor
                .CreateLocalized("LV.SE.SaveFrequency")
                .SetLocalizedTooltip("LV.SE.SaveFrequencyDesc")
                .SetEnableCondition(() => enabled.Value));

        saveCount = new RangeIntModSetting(
            5, 1, 20,
            ModSettingDescriptor
                .CreateLocalized("LV.SE.SaveCount")
                .SetLocalizedTooltip("LV.SE.SaveCountDesc")
                .SetEnableCondition(() => enabled.Value));

        AddCustomModSetting(enabled, nameof(enabled));
        AddCustomModSetting(saveFrequency, nameof(saveFrequency));
        AddCustomModSetting(saveCount, nameof(saveCount));

        enabled.ValueChanged += (_, _) => UpdateValues();
        saveFrequency.ValueChanged += (_, _) => UpdateValues();
        saveCount.ValueChanged += (_, _) => UpdateValues();

        UpdateValues();
    }

    void UpdateValues()
    {
        Enabled = enabled?.Value == true;
        SaveFrequency = saveFrequency?.Value ?? 1;
        SaveCount = saveCount?.Value ?? 3;
    }

}
