namespace PowerLines.Services;

[BindSingleton(Contexts = BindAttributeContext.MainMenu | BindAttributeContext.Game)]
public class MSettings(ISettings settings, ModSettingsOwnerRegistry modSettingsOwnerRegistry, ModRepository modRepository) : ModSettingsOwner(settings, modSettingsOwnerRegistry, modRepository)
{
    static readonly Color DefaultCableColor = new(1f, 216f / 255f, 0f);
    public override string ModId => nameof(PowerLines);
    public override ModSettingsContext ChangeableOn => ModSettingsContext.All;

    public ModSetting<bool> AlwaysShowCables { get; } = new(false, ModSettingDescriptor
        .CreateLocalized("LV.PL.AlwaysShowCables")
        .SetLocalizedTooltip("LV.PL.AlwaysShowCablesDesc")
    );

    public ColorModSetting CableColor { get; } = new(DefaultCableColor, ModSettingDescriptor
        .CreateLocalized("LV.PL.CableColor")
        .SetLocalizedTooltip("LV.PL.CableColorDesc"),
        false
    );

 
}