
namespace TImprove4Modders;

public class MSettings(ISettings settings, ModSettingsOwnerRegistry modSettingsOwnerRegistry, ModRepository modRepository)
    : ModSettingsOwner(settings, modSettingsOwnerRegistry, modRepository), IUnloadableSingleton
{
    public override string ModId => nameof(TImprove4Modders);
    public override ModSettingsContext ChangeableOn => ModSettingsContext.All;

    #region Settings

    readonly ModSetting<bool> swapBuildFinishedModifier = new(
        true,
        ModSettingDescriptor
            .CreateLocalized("LV.TIMod.SwapBuildFinishedModifier")
            .SetLocalizedTooltip("LV.TIMod.SwapBuildFinishedModifierDesc"));

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

    #endregion

    public static bool SwapBuildFinishedModifier { get; private set; }
    public static bool PickThumbnail { get; private set; }
    public static bool OpenExternalBrowser { get; private set; }
    public static bool DevModeOnDefault { get; private set; }
    public static bool QuickQuit { get; private set; }
    public static bool QuickRestart { get; private set; }
    public static bool NoClearDevFilter { get; private set; }

    public event Action OnSettingsChanged = delegate { };

    public override void OnAfterLoad()
    {
        AddCustomModSetting(swapBuildFinishedModifier, nameof(swapBuildFinishedModifier));
        AddCustomModSetting(noClearDevFilter, nameof(noClearDevFilter));
        AddCustomModSetting(pickThumbnail, nameof(pickThumbnail));
        AddCustomModSetting(openExternalBrowser, nameof(openExternalBrowser));
        AddCustomModSetting(devModeOnDefault, nameof(devModeOnDefault));
        AddCustomModSetting(quickQuit, nameof(quickQuit));
        AddCustomModSetting(quickRestart, nameof(quickRestart));

        swapBuildFinishedModifier.ValueChanged += (_, _) => InternalOnSettingsChanged();
        pickThumbnail.ValueChanged += (_, _) => InternalOnSettingsChanged();
        openExternalBrowser.ValueChanged += (_, _) => InternalOnSettingsChanged();
        devModeOnDefault.ValueChanged += (_, _) => InternalOnSettingsChanged();
        quickQuit.ValueChanged += (_, _) => InternalOnSettingsChanged();
        quickRestart.ValueChanged += (_, _) => InternalOnSettingsChanged();
        noClearDevFilter.ValueChanged += (_, _) => InternalOnSettingsChanged();

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

        OnSettingsChanged();
    }

    public void Unload()
    {
        InternalOnSettingsChanged();
    }
}
