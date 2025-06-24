namespace FiveMoreMins;

public class MSettings(ISettings settings, ModSettingsOwnerRegistry modSettingsOwnerRegistry, ModRepository modRepository) : ModSettingsOwner(settings, modSettingsOwnerRegistry, modRepository)
{
    public static float MaxDelayValue { get; private set; }
    public static float MinDelayValue { get; private set; }

    public override string ModId { get; } = nameof(FiveMoreMins);
    public override ModSettingsContext ChangeableOn { get; } = ModSettingsContext.All;

    public ModSetting<float> MaxDelay { get; } = new(1f, ModSettingDescriptor
        .CreateLocalized("LV.FMM.MaxDelay")
        .SetLocalizedTooltip("LV.FMM.MaxDelayDesc"));
    public ModSetting<float> MinDelay { get; } = new(0f, ModSettingDescriptor
        .CreateLocalized("LV.FMM.MinDelay")
        .SetLocalizedTooltip("LV.FMM.MinDelayDesc"));

    public override void OnAfterLoad()
    {
        MaxDelay.ValueChanged += (_, v) => MaxDelayValue = v;
        MinDelay.ValueChanged += (_, v) => MinDelayValue = v;

        MaxDelayValue = MaxDelay.Value;
        MinDelayValue = MinDelay.Value;
    }

}
