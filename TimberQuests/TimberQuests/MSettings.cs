namespace TimberQuests;

public class MSettings(ISettings settings, ModSettingsOwnerRegistry modSettingsOwnerRegistry, ModRepository modRepository) : ModSettingsOwner(settings, modSettingsOwnerRegistry, modRepository)
{

    public override string ModId { get; } = nameof(TimberQuests);

    public RangeIntModSetting UpdateFreq { get; } = new(
        5, 1, 15,
        ModSettingDescriptor
            .CreateLocalized("LV.TQ.UpdateFreq")
            .SetLocalizedTooltip("LV.TQ.UpdateFreqDesc")
    );

}
