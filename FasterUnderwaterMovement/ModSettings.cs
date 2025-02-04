global using Timberborn.WalkingSystem;

namespace FasterUnderwaterMovement;

public class ModSettings(ISettings settings, ModSettingsOwnerRegistry modSettingsOwnerRegistry, ModRepository modRepository) : ModSettingsOwner(settings, modSettingsOwnerRegistry, modRepository)
{
    static readonly FieldInfo SwimmingPenalty = typeof(WalkerSpeedManager).GetField("SwimmingPenalty", BindingFlags.NonPublic | BindingFlags.Static);

    protected override string ModId => nameof(FasterUnderwaterMovement);
    public override ModSettingsContext ChangeableOn => ModSettingsContext.All;

    protected override void OnAfterLoad()
    {
        ModSetting<float> bonus = new(.5f, ModSettingDescriptor
            .CreateLocalized("LV.FUM.SwimmingBonus")
            .SetLocalizedTooltip("LV.FUM.SwimmingBonusDesc"));

        AddCustomModSetting(bonus, nameof(bonus));

        bonus.ValueChanged += (_, value) => SetValue(value);
        SetValue(bonus.Value);
    }

    void SetValue(float bonus)
    {
        SwimmingPenalty.SetValue(null, -bonus);
    }
}
