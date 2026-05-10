namespace WaterErosion.Services;

[BindSingleton(Contexts = BindAttributeContext.All)]
public class MSettings(ISettings settings, ModSettingsOwnerRegistry modSettingsOwnerRegistry, ModRepository modRepository) : ModSettingsOwner(settings, modSettingsOwnerRegistry, modRepository)
{

    public override string ModId => nameof(WaterErosion);

    public ModSetting<float> ErosionRate { get; } = new(1f, Create("ErosionRate"));
    public ModSetting<float> BadwaterErosionRate { get; } = new(3f, Create("BadwaterErosionRate"));
    public RangeIntModSetting BlockageThreshold = new(50, 0, 90, Create("BlockageThreshold"));

    static ModSettingDescriptor Create(string key) => ModSettingDescriptor.CreateLocalized("LV.WE." + key)
        .SetLocalizedTooltip($"LV.WE.{key}Desc");

}
