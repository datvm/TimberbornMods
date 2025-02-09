namespace ConfigurableBeaverWalk;

public class ModSettings(
        ISettings settings,
        ModSettingsOwnerRegistry modSettingsOwnerRegistry,
        ModRepository modRepository
) : ModSettingsOwner(settings, modSettingsOwnerRegistry, modRepository), IUnloadableSingleton
{
    public override string ModId => nameof(ConfigurableBeaverWalk);
    public override ModSettingsContext ChangeableOn => ModSettingsContext.All;

    public static bool ChangeWalkingSpeed { get; private set; }

    public static float BaseWalkingSpeed { get; private set; }
    public static float BaseSlowedSpeed { get; private set; }

    public static bool DifferentForBots { get; private set; }
    public static float BaseBotWalkingSpeed { get; private set; }
    public static float BaseBotSlowedSpeed { get; private set; }

    public static float CarryingWeightMultiplier { get; private set; } = 1;

    RangeIntModSetting? baseWalkingSpeed, baseSlowedSpeed, baseBotWalkingSpeed, baseBotSlowedSpeed;
    ModSetting<float>? carryingWeightMultiplier;
    ModSetting<bool>? changeWalkingSpeed, differentBotSpeed;

    public override void OnAfterLoad()
    {
        base.OnAfterLoad();

        changeWalkingSpeed = new(
            true,
            ModSettingDescriptor.CreateLocalized("CBW.ChangeWalkingSpeed")
                .SetLocalizedTooltip("CBW.ChangeWalkingSpeedDesc"));

        baseWalkingSpeed = new(
            6, 0, 100,
            ModSettingDescriptor.CreateLocalized("CBW.BaseWalkingSpeed")
                .SetLocalizedTooltip("CBW.BaseWalkingSpeedDesc")
                .SetEnableCondition(() => changeWalkingSpeed.Value));

        baseSlowedSpeed = new(
            4, 0, 100,
            ModSettingDescriptor.CreateLocalized("CBW.BaseSlowedSpeed")
                .SetLocalizedTooltip("CBW.BaseSlowedSpeedDesc")
                .SetEnableCondition(() => changeWalkingSpeed.Value));

        differentBotSpeed = new(
            false,
            ModSettingDescriptor.CreateLocalized("CBW.DifferentBotSpeed")
                .SetLocalizedTooltip("CBW.DifferentBotSpeedDesc")
                .SetEnableCondition(() => changeWalkingSpeed.Value));

        baseBotWalkingSpeed = new(
            6, 0, 100,
            ModSettingDescriptor.CreateLocalized("CBW.BaseBotWalkingSpeed")
                .SetLocalizedTooltip("CBW.BaseBotWalkingSpeedDesc")
                .SetEnableCondition(() => changeWalkingSpeed.Value && differentBotSpeed.Value));

        baseBotSlowedSpeed = new(
            4, 0, 100,
            ModSettingDescriptor.CreateLocalized("CBW.BaseBotSlowedSpeed")
                .SetLocalizedTooltip("CBW.BaseBotSlowedSpeedDesc")
                .SetEnableCondition(() => changeWalkingSpeed.Value && differentBotSpeed.Value));

        carryingWeightMultiplier = new(
            1,
            ModSettingDescriptor.CreateLocalized("CBW.CarryingWeightMultiplier")
                .SetLocalizedTooltip("CBW.CarryingWeightMultiplierDesc"));

        AddCustomModSetting(changeWalkingSpeed, "beaver_change_walking_speed");
        AddCustomModSetting(baseWalkingSpeed, "beaver_base_walking_speed");
        AddCustomModSetting(baseSlowedSpeed, "beaver_base_slowed_speed");
        AddCustomModSetting(differentBotSpeed, "beaver_different_bot_speed");
        AddCustomModSetting(baseBotWalkingSpeed, "beaver_base_bot_walking_speed");
        AddCustomModSetting(baseBotSlowedSpeed, "beaver_base_bot_slowed_speed");
        AddCustomModSetting(carryingWeightMultiplier, "beaver_carrying_weight_multiplier");

        changeWalkingSpeed.ValueChanged += (_, _) => UpdateValues();
        baseWalkingSpeed.ValueChanged += (_, _) => UpdateValues();
        baseSlowedSpeed.ValueChanged += (_, _) => UpdateValues();
        differentBotSpeed.ValueChanged += (_, _) => UpdateValues();
        baseBotWalkingSpeed.ValueChanged += (_, _) => UpdateValues();
        baseBotSlowedSpeed.ValueChanged += (_, _) => UpdateValues();
        carryingWeightMultiplier.ValueChanged += (_, _) => UpdateValues();

        UpdateValues();
    }

    void UpdateValues()
    {
        ChangeWalkingSpeed = changeWalkingSpeed?.Value == true;
        BaseWalkingSpeed = baseWalkingSpeed?.Value ?? 0;
        BaseSlowedSpeed = baseSlowedSpeed?.Value ?? 0;
        DifferentForBots = differentBotSpeed?.Value == true;
        BaseBotWalkingSpeed = baseBotWalkingSpeed?.Value ?? 0;
        BaseBotSlowedSpeed = baseBotSlowedSpeed?.Value ?? 0;
        CarryingWeightMultiplier = carryingWeightMultiplier?.Value ?? 1;
    }

    public void Unload()
    {
        UpdateValues();
    }
}