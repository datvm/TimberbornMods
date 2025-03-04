namespace ConfigurableIrrigation;

public class MSettings(ISettings settings, ModSettingsOwnerRegistry modSettingsOwnerRegistry, ModRepository modRepository)
    : ModSettingsOwner(settings, modSettingsOwnerRegistry, modRepository), IUnloadableSingleton
{
    public override string ModId => nameof(ConfigurableIrrigation);

    public static float MinimumWaterContamination { get; private set; } = 0.01f;
    public static float MaximumWaterContamination { get; private set; } = 0.53f;
    public static int VerticalSpreadCostMultiplier { get; private set; } = 3;
    public static int MaxSpread { get; private set; } = 24;
    public static float ContaMinimumWaterContamination { get; private set; } = 0.5f;
    public static int ContaMaxSpread { get; private set; } = 7;
    public static int ContaVerticalSpreadCostMultiplier { get; private set; } = 5;

    readonly ModSetting<float> minimumWaterContamination = new(
        0.01f,
        ModSettingDescriptor
            .CreateLocalized("LV.CI.MinimumWaterContamination")
            .SetLocalizedTooltip("LV.CI.MinimumWaterContaminationDesc"));

    readonly ModSetting<float> maximumWaterContamination = new(
        0.53f,
        ModSettingDescriptor
            .CreateLocalized("LV.CI.MaximumWaterContamination")
            .SetLocalizedTooltip("LV.CI.MaximumWaterContaminationDesc"));

    readonly ModSetting<int> verticalSpreadCostMultiplier = new(
        3,
        ModSettingDescriptor
            .CreateLocalized("LV.CI.VerticalSpreadCostMultiplier")
            .SetLocalizedTooltip("LV.CI.VerticalSpreadCostMultiplierDesc"));

    readonly ModSetting<int> maxSpread = new(
        24,
        ModSettingDescriptor
            .CreateLocalized("LV.CI.MaxSpread")
            .SetLocalizedTooltip("LV.CI.MaxSpreadDesc"));

    readonly ModSetting<float> contaMinimumWaterContamination = new(
        0.5f,
        ModSettingDescriptor
            .CreateLocalized("LV.CI.ContaMinimumWaterContamination")
            .SetLocalizedTooltip("LV.CI.ContaMinimumWaterContaminationDesc"));

    readonly ModSetting<int> contaMaxSpread = new(
        7,
        ModSettingDescriptor
            .CreateLocalized("LV.CI.ContaMaxSpread")
            .SetLocalizedTooltip("LV.CI.ContaMaxSpreadDesc"));

    readonly ModSetting<int> contaVerticalSpreadCostMultiplier = new(
        5,
        ModSettingDescriptor
            .CreateLocalized("LV.CI.ContaVerticalSpreadCostMultiplier")
            .SetLocalizedTooltip("LV.CI.ContaVerticalSpreadCostMultiplierDesc"));

    public override void OnAfterLoad()
    {
        AddCustomModSetting(minimumWaterContamination, nameof(MinimumWaterContamination));
        AddCustomModSetting(maximumWaterContamination, nameof(MaximumWaterContamination));
        AddCustomModSetting(verticalSpreadCostMultiplier, nameof(VerticalSpreadCostMultiplier));
        AddCustomModSetting(maxSpread, nameof(MaxSpread));
        AddCustomModSetting(contaMinimumWaterContamination, nameof(ContaMinimumWaterContamination));
        AddCustomModSetting(contaMaxSpread, nameof(ContaMaxSpread));
        AddCustomModSetting(contaVerticalSpreadCostMultiplier, nameof(ContaVerticalSpreadCostMultiplier));

        UpdateValues();
    }

    public void Unload()
    {
        UpdateValues();
    }

    void UpdateValues()
    {
        MinimumWaterContamination = minimumWaterContamination.Value;
        MaximumWaterContamination = maximumWaterContamination.Value;
        VerticalSpreadCostMultiplier = verticalSpreadCostMultiplier.Value;
        MaxSpread = maxSpread.Value;
        ContaMinimumWaterContamination = contaMinimumWaterContamination.Value;
        ContaMaxSpread = contaMaxSpread.Value;
        ContaVerticalSpreadCostMultiplier = contaVerticalSpreadCostMultiplier.Value;
    }
}
