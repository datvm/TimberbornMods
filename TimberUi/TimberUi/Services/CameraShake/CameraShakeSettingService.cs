namespace TimberUi.Services;

public class CameraShakeSettingService(
    ISettings settings,
    ISettingsController iSettingsController,
    ILoc t
) : ILoadableSingleton
{
    public const string CameraShakeDisabledKey = $"{nameof(TimberUi)}.CameraShakeDisabled";
    
    public event Action<bool>? OnCameraShakeDisabledChanged;
    readonly SettingsBox settingsBox = (SettingsBox)iSettingsController;

    public bool IsDisabled { get; private set; }

    public void Load()
    {
        IsDisabled = settings.GetSafeBool(CameraShakeDisabledKey, false);
        AddSettingOption();
    }

    public void SetDisabledCamera(bool disabled)
    {
        if (disabled == IsDisabled) { return; }

        settings.SetBool(CameraShakeDisabledKey, disabled);
        
        IsDisabled = disabled;
        OnCameraShakeDisabledChanged?.Invoke(disabled);
    }

    void AddSettingOption()
    {
        var root = settingsBox._root;
        var chkDisableCS = root.AddToggle("LV.TimberUi.DisableCameraShake".T(t), onValueChanged: SetDisabledCamera);
        chkDisableCS.SetValueWithoutNotify(IsDisabled);

        var uiScaleFactor = root.Q("UIScaleFactor");
        chkDisableCS.InsertSelfAfter(uiScaleFactor);
    }

}
