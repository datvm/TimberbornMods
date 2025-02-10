global using UnityEngine.InputSystem;

namespace QuickStart;

public class MSettings(ISettings settings, ModSettingsOwnerRegistry modSettingsOwnerRegistry, ModRepository modRepository)
    : ModSettingsOwner(settings, modSettingsOwnerRegistry, modRepository), IUnloadableSingleton
{

    public override string ModId => nameof(QuickStart);

    readonly ModSetting<bool> autoContinueShift = new(false, ModSettingDescriptor
        .CreateLocalized("LV.QS.AutoContinueShift")
        .SetLocalizedTooltip("LV.QS.AutoContinueShiftDesc"));
    readonly ModSetting<bool> autoContinueShiftToCancel = new(false, ModSettingDescriptor
        .CreateLocalized("LV.QS.AutoContinueShiftToCancel")
        .SetLocalizedTooltip("LV.QS.AutoContinueShiftToCancelDesc"));

    static bool firstTime = true;
    static bool init = false;
    public static MainMenuPanel? MainMenuPanel { get; set; }

    public static bool AutoContinueShift { get; private set; } = false;
    public static bool AutoContinueShiftToCancel { get; private set; } = false;

    public override void OnAfterLoad()
    {
        autoContinueShiftToCancel.Descriptor
            .SetEnableCondition(() => autoContinueShift.Value);

        AddCustomModSetting(autoContinueShift, nameof(autoContinueShift));
        AddCustomModSetting(autoContinueShiftToCancel, nameof(autoContinueShiftToCancel));

        UpdateValues();
        init = true;

        CheckAutoLoading();
    }

    public static void CheckAutoLoading()
    {
        if (!init || !firstTime || MainMenuPanel is null) { return; }
        firstTime = false;

        if (!AutoContinueShift) { return; }

        var shiftHolding = Keyboard.current.shiftKey.isPressed;
        
        if ((shiftHolding && AutoContinueShiftToCancel) ||
            (!shiftHolding && !AutoContinueShiftToCancel)) { return; }

        MainMenuPanel.ContinueClicked(null!);
    }

    void UpdateValues()
    {
        AutoContinueShift = autoContinueShift.Value;
        AutoContinueShiftToCancel = autoContinueShiftToCancel.Value;
    }

    public void Unload()
    {
        firstTime = false;
        MainMenuPanel = null;

        UpdateValues();
    }
}
