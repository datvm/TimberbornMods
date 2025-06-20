namespace FiveMoreMins;

public class MSettings(ISettings settings, ModSettingsOwnerRegistry modSettingsOwnerRegistry, ModRepository modRepository) : ModSettingsOwner(settings, modSettingsOwnerRegistry, modRepository)
{
    public static float MaxDelayValue { get; private set; }

    public override string ModId { get; } = nameof(FiveMoreMins);
    public override ModSettingsContext ChangeableOn { get; } = ModSettingsContext.All;

    public ModSetting<float> MaxDelay { get; } = new(1f, ModSettingDescriptor
        .CreateLocalized("LV.FMM.MaxDelay")
        .SetLocalizedTooltip("LV.FMM.MaxDelayDesc"));

    public override void OnAfterLoad()
    {
        MaxDelay.ValueChanged += (_, v) => MaxDelayValue = v;
        MaxDelayValue = MaxDelay.Value;
    }

}
