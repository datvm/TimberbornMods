namespace ScenarioEditor;

public class MSettings(ISettings settings, ModSettingsOwnerRegistry modSettingsOwnerRegistry, ModRepository modRepository) : ModSettingsOwner(settings, modSettingsOwnerRegistry, modRepository)
{

    public override string ModId { get; } = nameof(ScenarioEditor);
    public override ModSettingsContext ChangeableOn { get; } = ModSettingsContext.All;

    public ModSetting<bool> CameraShake { get; } = new(true, ModSettingDescriptor
        .CreateLocalized("LV.ScE.CameraShake")
        .SetLocalizedTooltip("LV.ScE.CameraShakeDesc"));

}
