namespace ConstantWind;

public class MSettings(ISettings settings, ModSettingsOwnerRegistry modSettingsOwnerRegistry, ModRepository modRepository) : ModSettingsOwner(settings, modSettingsOwnerRegistry, modRepository)
{

    public ModSetting<int> WindStrength { get; } = new(50,
        ModSettingDescriptor.CreateLocalized("CW.WindStrength")
            .SetLocalizedTooltip("CW.WindStrengthDesc"));

    public override string ModId { get; } = nameof(ConstantWind);
    public override ModSettingsContext ChangeableOn { get; } = ModSettingsContext.All;

}
