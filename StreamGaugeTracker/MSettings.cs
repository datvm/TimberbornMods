namespace StreamGaugeTracker;

public class MSettings(ISettings settings, ModSettingsOwnerRegistry modSettingsOwnerRegistry, ModRepository modRepository) : ModSettingsOwner(settings, modSettingsOwnerRegistry, modRepository)
{

    public override string ModId { get; } = nameof(StreamGaugeTracker);
    public override ModSettingsContext ChangeableOn { get; } = ModSettingsContext.All;

    public ModSetting<int> SamplingFreq { get; } = new(6, ModSettingDescriptor
        .CreateLocalized("LV.SGT.SamplingFreq")
        .SetLocalizedTooltip("LV.SGT.SamplingFreqDesc"));
    public ModSetting<int> SamplingCount { get; } = new(120, ModSettingDescriptor
        .CreateLocalized("LV.SGT.SamplingCount")
        .SetLocalizedTooltip("LV.SGT.SamplingCountDesc"));
    public ModSetting<int> SamplesPerBar { get; } = new(1, ModSettingDescriptor
        .CreateLocalized("LV.SGT.SamplesPerBar")
        .SetLocalizedTooltip("LV.SGT.SamplesPerBarDesc"));

}
