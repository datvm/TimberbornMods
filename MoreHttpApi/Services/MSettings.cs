namespace MoreHttpApi.Services;

[BindSingleton(Contexts = BindAttributeContext.All)]
public class MSettings(ISettings settings, ModSettingsOwnerRegistry modSettingsOwnerRegistry, ModRepository modRepository) : ModSettingsOwner(settings, modSettingsOwnerRegistry, modRepository)
{

    public override string ModId => nameof(MoreHttpApi);
    public override ModSettingsContext ChangeableOn => ModSettingsContext.All;

    public ModSetting<bool> AutoStartApi { get; } = new(false, ModSettingDescriptor
        .CreateLocalized("LV.MHA.AutoStart")
        .SetLocalizedTooltip("LV.MHA.AutoStartDesc"));

    public ModSetting<int> AutoStartPort { get; } = new(8080, ModSettingDescriptor
        .CreateLocalized("LV.MHA.AutoStartPort")
        .SetLocalizedTooltip("LV.MHA.AutoStartPortDesc"));

    public override void OnBeforeLoad()
    {
        base.OnBeforeLoad();
        AutoStartPort.Descriptor.SetEnableCondition(() => AutoStartApi.Value);
    }

}
