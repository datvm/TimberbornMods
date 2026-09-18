namespace TimberPipes.Services;

[BindSingleton(Contexts = BindAttributeContext.MainMenu | BindAttributeContext.Game)]
public class MSettings(ISettings settings, ModSettingsOwnerRegistry modSettingsOwnerRegistry, ModRepository modRepository) : ModSettingsOwner(settings, modSettingsOwnerRegistry, modRepository)
{

    public override string ModId => nameof(TimberPipes);

    public ModSetting<float> EqualizeK { get; } = new(0.8f, Create("EqualizeK"));

    static ModSettingDescriptor Create(string key) => ModSettingDescriptor.CreateLocalized("LV.TPi." + key)
        .SetLocalizedTooltip($"LV.TPi.{key}Desc");

}
