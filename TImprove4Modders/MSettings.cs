namespace TImprove4Modders;

public class MSettings(
    ISettings settings,
    ModSettingsOwnerRegistry modSettingsOwnerRegistry,
    ModRepository modRepository,
    ISystemFileDialogService systemFileDialogService
)
    : ModSettingsOwner(settings, modSettingsOwnerRegistry, modRepository), IUnloadableSingleton
{
    public override string ModId => nameof(TImprove4Modders);
    public override ModSettingsContext ChangeableOn => ModSettingsContext.All;

    public static ISystemFileDialogService? SystemFileDialogService { get; private set; }

    #region Settings

    readonly ModSetting<bool> swapBuildFinishedModifier = new(
        true,
        ModSettingDescriptor
            .CreateLocalized("LV.TIMod.SwapBuildFinishedModifier")
            .SetLocalizedTooltip("LV.TIMod.SwapBuildFinishedModifierDesc"));

    readonly ModSetting<bool> betterModOrder = new(
        true,
        ModSettingDescriptor
            .CreateLocalized("LV.TIMod.BetterResetLoadOrder")
            .SetLocalizedTooltip("LV.TIMod.BetterResetLoadOrderDesc"));

    readonly ModSetting<bool> pickThumbnail = new(
        true,
        ModSettingDescriptor
            .CreateLocalized("LV.TIMod.PickThumbnail")
            .SetLocalizedTooltip("LV.TIMod.PickThumbnailDesc"));

    readonly ModSetting<bool> openExternalBrowser = new(
        true,
        ModSettingDescriptor
            .CreateLocalized("LV.TIMod.OpenExternalBrowser")
            .SetLocalizedTooltip("LV.TIMod.OpenExternalBrowserDesc"));

    readonly ModSetting<bool> devModeOnDefault = new(
        true,
        ModSettingDescriptor
            .CreateLocalized("LV.TIMod.DevModeOnDefault")
            .SetLocalizedTooltip("LV.TIMod.DevModeOnDefaultDesc"));

    readonly ModSetting<bool> quickQuit = new(
        true,
        ModSettingDescriptor
            .CreateLocalized("LV.TIMod.QuickQuit")
            .SetLocalizedTooltip("LV.TIMod.QuickQuitDesc"));

    readonly ModSetting<bool> quickRestart = new(
        true,
        ModSettingDescriptor
            .CreateLocalized("LV.TIMod.QuickRestart")
            .SetLocalizedTooltip("LV.TIMod.QuickRestartDesc"));

    readonly ModSetting<bool> noClearDevFilter = new(
        true,
        ModSettingDescriptor
            .CreateLocalized("LV.TIMod.NoClearDevFilter")
            .SetLocalizedTooltip("LV.TIMod.NoClearDevFilterDesc"));

    readonly ModSetting<bool> noExitSave = new(
        false,
        ModSettingDescriptor
            .CreateLocalized("LV.TIMod.NoExitSave")
            .SetLocalizedTooltip("LV.TIMod.NoExitSaveDesc"));

    #endregion

    public static bool SwapBuildFinishedModifier { get; private set; }
    public static bool PickThumbnail { get; private set; }
    public static bool OpenExternalBrowser { get; private set; }
    public static bool DevModeOnDefault { get; private set; }
    public static bool QuickQuit { get; private set; }
    public static bool QuickRestart { get; private set; }
    public static bool NoClearDevFilter { get; private set; }
    public static bool BetterModOrder { get; private set; }
    public static bool NoExitSave { get; private set; }

    public override void OnAfterLoad()
    {
        SystemFileDialogService = systemFileDialogService;

        AddCustomModSetting(swapBuildFinishedModifier, nameof(swapBuildFinishedModifier));
        AddCustomModSetting(noClearDevFilter, nameof(noClearDevFilter));
        AddCustomModSetting(pickThumbnail, nameof(pickThumbnail));
        AddCustomModSetting(openExternalBrowser, nameof(openExternalBrowser));
        AddCustomModSetting(devModeOnDefault, nameof(devModeOnDefault));
        AddCustomModSetting(betterModOrder, nameof(betterModOrder));
        AddCustomModSetting(noExitSave, nameof(noExitSave));
        AddCustomModSetting(quickQuit, nameof(quickQuit));
        AddCustomModSetting(quickRestart, nameof(quickRestart));

        ModSettingChanged += (_, _) => InternalOnSettingsChanged();
        InternalOnSettingsChanged();
    }

    void InternalOnSettingsChanged()
    {
        PickThumbnail = pickThumbnail.Value;
        OpenExternalBrowser = openExternalBrowser.Value;
        DevModeOnDefault = devModeOnDefault.Value;
        QuickQuit = quickQuit.Value;
        QuickRestart = quickRestart.Value;
        SwapBuildFinishedModifier = swapBuildFinishedModifier.Value;
        NoClearDevFilter = noClearDevFilter.Value;
        BetterModOrder = betterModOrder.Value;
        NoExitSave = noExitSave.Value;

    }

    public void Unload()
    {
        InternalOnSettingsChanged();
        SystemFileDialogService = null;
    }
}
