namespace ConfigurableLifetime;

public class MSettings(
  ISettings settings,
  ModSettingsOwnerRegistry modSettingsOwnerRegistry,
  ModRepository modRepository) : ModSettingsOwner(settings, modSettingsOwnerRegistry, modRepository), IUnloadableSingleton
{

    public static MSettings? Instance { get; private set; }

    public override string ModId { get; } = nameof(ConfigurableLifetime);
    public override ModSettingsContext ChangeableOn { get; } = ModSettingsContext.All;

    public ModSetting<float> BeaverLifeMul { get; } = new(1f, ModSettingDescriptor
        .CreateLocalized("LV.CLt.BeaverLifeMul")
        .SetLocalizedTooltip("LV.CLt.BeaverLifeMulDesc"));
    public ModSetting<float> ChildhoodDaysMul { get; } = new(1f, ModSettingDescriptor
        .CreateLocalized("LV.CLt.ChildhoodDaysMul")
        .SetLocalizedTooltip("LV.CLt.ChildhoodDaysMulDesc"));
    public ModSetting<float> BotLifeMul { get; } = new(1.0f, ModSettingDescriptor
        .CreateLocalized("LV.CLt.BotLifeMul")
        .SetLocalizedTooltip("LV.CLt.BotLifeMulDesc"));

    public override void OnAfterLoad()
    {
        base.OnAfterLoad();
        Instance = this;
    }

    public void Unload()
    {
        Instance = null;
    }
}
