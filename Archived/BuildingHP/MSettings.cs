namespace BuildingHP;

public class MSettings(
    ISettings settings,
    ModSettingsOwnerRegistry modSettingsOwnerRegistry,
    ModRepository modRepository,
    IModSettingsContextProvider contextProvider
) : ModSettingsOwner(settings, modSettingsOwnerRegistry, modRepository)
{

    public override string ModId { get; } = nameof(BuildingHP);
    public override ModSettingsContext ChangeableOn { get; } = ModSettingsContext.All;

    public static bool EnableMaintenanceValue { get; private set; }
    public static bool EnableMaterialDurabilityValue { get; private set; }
    public static float MaintenanceHPDaysValue { get; private set; }

    public ModSetting<bool> EnableMaintenance { get; } = new(false, ModSettingDescriptor
        .CreateLocalized("LV.BHP.EnableMaintenance")
        .SetLocalizedTooltip("LV.BHP.EnableMaintenanceDesc"));

    public ModSetting<float> MaintenanceHPDays { get; } = new(1f, ModSettingDescriptor
        .CreateLocalized("LV.BHP.MaintenanceHPDays")
        .SetLocalizedTooltip("LV.BHP.MaintenanceHPDaysDesc"));

    public ModSetting<bool> EnableMaterialDurability { get; } = new(true, ModSettingDescriptor
        .CreateLocalized("LV.BHP.EnableMaterialDurability")
        .SetLocalizedTooltip("LV.BHP.EnableMaterialDurabilityDesc"));

    public ModSetting<bool> EnableWarnLowHp { get; } = new(true, ModSettingDescriptor
        .CreateLocalized("LV.BHP.EnableWarnLowHp")
        .SetLocalizedTooltip("LV.BHP.EnableWarnLowHpDesc"));

    public ModSetting<int> LowHpThreshold { get; } = new(5, ModSettingDescriptor
        .CreateLocalized("LV.BHP.LowHpThreshold")
        .SetLocalizedTooltip("LV.BHP.LowHpThresholdDesc"));

    public ModSetting<bool> EnableWarnLowHpPerc { get; } = new(true, ModSettingDescriptor
        .CreateLocalized("LV.BHP.EnableWarnLowHpPerc")
        .SetLocalizedTooltip("LV.BHP.EnableWarnLowHpPercDesc"));

    public RangeIntModSetting LowHpPercThreshold { get; } = new(25, 1, 100, ModSettingDescriptor
        .CreateLocalized("LV.BHP.LowHpPercThreshold")
        .SetLocalizedTooltip("LV.BHP.LowHpPercThresholdDesc"));

    public ModSetting<bool> AutoRepairOn { get; } = new(true, ModSettingDescriptor
        .CreateLocalized("LV.BHP.AutoRepairOn")
        .SetLocalizedTooltip("LV.BHP.AutoRepairOnDesc"));

    public RangeIntModSetting AutoRepairDefaultThreshold { get; } = new(50, 1, 100, ModSettingDescriptor
        .CreateLocalized("LV.BHP.AutoRepairDefaultThreshold")
        .SetLocalizedTooltip("LV.BHP.AutoRepairDefaultThresholdDesc"));

    public override void OnBeforeLoad()
    {
        base.OnBeforeLoad();

        var isMenuContext = contextProvider.Context == ModSettingsContext.MainMenu;
        EnableMaintenance.Descriptor.SetEnableCondition(() => isMenuContext);
        EnableMaterialDurability.Descriptor.SetEnableCondition(() => isMenuContext);

        MaintenanceHPDays.Descriptor.SetEnableCondition(() => EnableMaintenance.Value);
    }

    public override void OnAfterLoad()
    {
        base.OnAfterLoad();

        EnableMaintenance.ValueChanged += (_, v) => EnableMaintenanceValue = v;
        EnableMaterialDurability.ValueChanged += (_, v) => EnableMaterialDurabilityValue = v;
        MaintenanceHPDays.ValueChanged += (_, v) => MaintenanceHPDaysValue = v;

        EnableMaintenanceValue = EnableMaintenance.Value;
        EnableMaterialDurabilityValue = EnableMaterialDurability.Value;
        MaintenanceHPDaysValue = MaintenanceHPDays.Value;
    }

}
