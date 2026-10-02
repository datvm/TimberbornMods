namespace ConveyorBelt.Services;

[BindSingleton(Contexts = BindAttributeContext.Game | BindAttributeContext.MainMenu)]
public class MSettings(ISettings settings, ModSettingsOwnerRegistry modSettingsOwnerRegistry, ModRepository modRepository) : ModSettingsOwner(settings, modSettingsOwnerRegistry, modRepository)
{
    public override string ModId => nameof(ConveyorBelt);
    public override ModSettingsContext ChangeableOn => ModSettingsContext.All;

    public ModSetting<bool> Animation { get; } = new(true, ModSettingDescriptor
        .CreateLocalized("LV.CBlt.Animation")
        .SetLocalizedTooltip("LV.CBlt.AnimationDesc"));

    public ModSetting<bool> AlwaysShowArrows { get; } = new(false, ModSettingDescriptor
        .CreateLocalized("LV.CBlt.AlwaysShowArrows")
        .SetLocalizedTooltip("LV.CBlt.AlwaysShowArrowsDesc"));
}
