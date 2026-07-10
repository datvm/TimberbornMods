namespace ConveyorBelt.Services;

[BindSingleton(Contexts = BindAttributeContext.Game | BindAttributeContext.MainMenu)]
public class MSettings(ISettings settings, ModSettingsOwnerRegistry modSettingsOwnerRegistry, ModRepository modRepository) : ModSettingsOwner(settings, modSettingsOwnerRegistry, modRepository)
{

    public override string ModId => nameof(ConveyorBelt);

    public ModSetting<bool> NoPower { get; } = new(false, ModSettingDescriptor
        .CreateLocalized("LV.CBlt.NoPower")
        .SetLocalizedTooltip("LV.CBlt.NoPowerDesc"));

    public ModSetting<bool> EarlierAvailability { get; } = new(false, ModSettingDescriptor
        .CreateLocalized("LV.CBlt.EarlierAvailability")
        .SetLocalizedTooltip("LV.CBlt.EarlierAvailabilityDesc"));

    bool? shouldModifyTemplates;
    public bool ShouldModifyTemplates => shouldModifyTemplates ??= NoPower.Value || EarlierAvailability.Value;

}
