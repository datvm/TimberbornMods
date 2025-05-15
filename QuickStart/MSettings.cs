namespace QuickStart;

public class MSettings(
    ISettings settings,
    ModSettingsOwnerRegistry modSettingsOwnerRegistry,
    ModRepository modRepository
)
    : ModSettingsOwner(settings, modSettingsOwnerRegistry, modRepository), IUnloadableSingleton
{

    public override string ModId { get; } = nameof(QuickStart);

    readonly ModSetting<bool> autoContinueShift = new(false, ModSettingDescriptor
        .CreateLocalized("LV.QS.AutoContinueShift")
        .SetLocalizedTooltip("LV.QS.AutoContinueShiftDesc"));
    readonly ModSetting<bool> autoContinueShiftToCancel = new(false, ModSettingDescriptor
        .CreateLocalized("LV.QS.AutoContinueShiftToCancel")
        .SetLocalizedTooltip("LV.QS.AutoContinueShiftToCancelDesc"));
    readonly ModSetting<bool> autoLoadMap = new(false, ModSettingDescriptor
        .CreateLocalized("LV.QS.AutoLoadMap")
        .SetLocalizedTooltip("LV.QS.AutoLoadMapDesc"));

    public static bool AutoContinueShift { get; private set; } = false;
    public static bool AutoContinueShiftToCancel { get; private set; } = false;
    public static bool AutoLoadMap { get; private set; } = false;

    public override void OnAfterLoad()
    {
        autoContinueShiftToCancel.Descriptor
            .SetEnableCondition(() => autoContinueShift.Value);
        autoLoadMap.Descriptor
            .SetEnableCondition(() => autoContinueShift.Value);

        AddCustomModSetting(autoContinueShift, nameof(autoContinueShift));
        AddCustomModSetting(autoContinueShiftToCancel, nameof(autoContinueShiftToCancel));
        AddCustomModSetting(autoLoadMap, nameof(autoLoadMap));

        UpdateValues();
    }

    void UpdateValues()
    {
        AutoContinueShift = autoContinueShift.Value;
        AutoContinueShiftToCancel = autoContinueShiftToCancel.Value;
        AutoLoadMap = autoLoadMap.Value;
    }

    public void Unload()
    {
        UpdateValues();
    }
}
