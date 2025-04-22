
namespace TImprove4Modders;

public class MSettings(
    ISettings settings,
    ModSettingsOwnerRegistry modSettingsOwnerRegistry,
    ModRepository modRepository,
    ModManagerBox modManagerBox
)
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

    readonly ModSetting<bool> tallerModManBox = new(
        false,
        ModSettingDescriptor
            .CreateLocalized("LV.TIMod.TallerModManBox")
            .SetLocalizedTooltip("LV.TIMod.TallerModManBoxDesc"));

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
    public static bool TallerModManBox { get; private set; }

    public override void OnAfterLoad()
    {
        AddCustomModSetting(swapBuildFinishedModifier, nameof(swapBuildFinishedModifier));
        AddCustomModSetting(noClearDevFilter, nameof(noClearDevFilter));
        AddCustomModSetting(pickThumbnail, nameof(pickThumbnail));
        AddCustomModSetting(openExternalBrowser, nameof(openExternalBrowser));
        AddCustomModSetting(devModeOnDefault, nameof(devModeOnDefault));
        AddCustomModSetting(betterModOrder, nameof(betterModOrder));
        AddCustomModSetting(tallerModManBox, nameof(tallerModManBox));
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
        TallerModManBox = tallerModManBox.Value;

        UpdateModManagerBox();
    }

    void UpdateModManagerBox()
    {
        var box = modManagerBox._root.Q(className: "mod-manager-box");

        box.style.height = TallerModManBox
            ? new StyleLength(new Length(90, LengthUnit.Percent))
            : StyleKeyword.Null;
    }

    int GetScreenHeightScaled()
    {
        return Mathf.FloorToInt(Screen.height / modManagerBox._root.scaledPixelsPerPoint);
    }

    public void Unload()
    {
        InternalOnSettingsChanged();
    }
}
