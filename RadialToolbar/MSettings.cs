namespace RadialToolbar;

[BindSingleton(Contexts = BindAttributeContext.All)]
public class MSettings(ISettings settings, ModSettingsOwnerRegistry modSettingsOwnerRegistry, ModRepository modRepository) : ModSettingsOwner(settings, modSettingsOwnerRegistry, modRepository)
{
    public override string ModId => nameof(RadialToolbar);
    public override ModSettingsContext ChangeableOn => ModSettingsContext.All;

    public ModSetting<bool> EightSegment { get; } = new(true, ModSettingDescriptor
        .CreateLocalized("LV.RT.EightSegment")
        .SetLocalizedTooltip("LV.RT.EightSegmentDesc"));

    public ModSetting<bool> FlattenSubmenus { get; } = new(true, ModSettingDescriptor
        .CreateLocalized("LV.RT.FlattenSubmenus")
        .SetLocalizedTooltip("LV.RT.FlattenSubmenusDesc"));

    public ModSetting<bool> HideBottomBar { get; } = new(false, ModSettingDescriptor
        .CreateLocalized("LV.RT.HideBottomBar")
        .SetLocalizedTooltip("LV.RT.HideBottomBarDesc"));

    public int SegmentCount => EightSegment.Value ? 8 : 4;
    public bool FlattenSubmenusValue => FlattenSubmenus.Value;

}
