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
    public static float WorkSpeedMultiplier { get; private set; } = 1;

    public static bool DifferentForBots { get; private set; }
    public static float BaseBotWalkingSpeed { get; private set; }
    public static float BaseBotSlowedSpeed { get; private set; }
    public static float BotWorkSpeedMultiplier { get; private set; } = 1;

    public static float CarryingWeightMultiplier { get; private set; } = 1;

    RangeIntModSetting? baseWalkingSpeed, baseSlowedSpeed, baseBotWalkingSpeed, baseBotSlowedSpeed;
    ModSetting<float>? carryingWeightMultiplier, workSpeedMultiplier, botWorkSpeedMultiplier;
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

        workSpeedMultiplier = new(
            1,
            ModSettingDescriptor.CreateLocalized("CBW.WorkSpeedMultiplier")
                .SetLocalizedTooltip("CBW.WorkSpeedMultiplierDesc"));

        differentBotSpeed = new(
            false,
            ModSettingDescriptor.CreateLocalized("CBW.DifferentBotSpeed")
                .SetLocalizedTooltip("CBW.DifferentBotSpeedDesc"));

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

        botWorkSpeedMultiplier = new(
            1,
            ModSettingDescriptor.CreateLocalized("CBW.BotWorkSpeedMultiplier")
                .SetLocalizedTooltip("CBW.BotWorkSpeedMultiplierDesc")
                .SetEnableCondition(() => differentBotSpeed.Value));

        carryingWeightMultiplier = new(
            1,
            ModSettingDescriptor.CreateLocalized("CBW.CarryingWeightMultiplier")
                .SetLocalizedTooltip("CBW.CarryingWeightMultiplierDesc"));

        AddCustomModSetting(changeWalkingSpeed, "beaver_change_walking_speed");
        AddCustomModSetting(baseWalkingSpeed, "beaver_base_walking_speed");
        AddCustomModSetting(baseSlowedSpeed, "beaver_base_slowed_speed");
        AddCustomModSetting(workSpeedMultiplier, "beaver_work_speed_multiplier");
        AddCustomModSetting(differentBotSpeed, "beaver_different_bot_speed");
        AddCustomModSetting(baseBotWalkingSpeed, "beaver_base_bot_walking_speed");
        AddCustomModSetting(baseBotSlowedSpeed, "beaver_base_bot_slowed_speed");
        AddCustomModSetting(botWorkSpeedMultiplier, "beaver_bot_work_speed_multiplier");
        AddCustomModSetting(carryingWeightMultiplier, "beaver_carrying_weight_multiplier");

        ModSettingChanged += (_, _) => UpdateValues();
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
        WorkSpeedMultiplier = workSpeedMultiplier?.Value ?? 1;
        BotWorkSpeedMultiplier = botWorkSpeedMultiplier?.Value ?? 1;
    }

    public void Unload()
    {
        UpdateValues();
    }
}