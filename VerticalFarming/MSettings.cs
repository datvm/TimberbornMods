namespace VerticalFarming;

public class MSettings(
    ISettings settings,
    ModSettingsOwnerRegistry modSettingsOwnerRegistry,
    ModRepository modRepository
) : ModSettingsOwner(settings, modSettingsOwnerRegistry, modRepository), IUnloadableSingleton
{

    public static bool RemoveCorner { get; private set; } = true;
    public static bool RemovePath { get; private set; } = true;
    public static bool WithoutGround { get; private set; } = true;

    public override string ModId { get; } = nameof(VerticalFarming);

    readonly ModSetting<bool> removeCorner = new(
        true,
        ModSettingDescriptor.CreateLocalized("LV.VF.RemoveCorner")
            .SetLocalizedTooltip("LV.VF.RemoveCornerDesc"));

    readonly ModSetting<bool> removePath = new(
        true,
        ModSettingDescriptor.CreateLocalized("LV.VF.RemovePath")
            .SetLocalizedTooltip("LV.VF.RemovePathDesc"));

    readonly ModSetting<bool> withoutGround = new(
        true,
        ModSettingDescriptor.CreateLocalized("LV.VF.WithoutGround")
            .SetLocalizedTooltip("LV.VF.WithoutGroundDesc"));

    public override void OnAfterLoad()
    {
        AddCustomModSetting(removeCorner, nameof(removeCorner));
        AddCustomModSetting(removePath, nameof(removePath));
        AddCustomModSetting(withoutGround, nameof(withoutGround));

        UpdateValues();
    }

    void UpdateValues()
    {
        RemoveCorner = removeCorner.Value;
        RemovePath = removePath.Value;
        WithoutGround = withoutGround.Value;
    }

    public void Unload()
    {
        UpdateValues();
    }
}
