using ModSettings.Common;
using ModSettings.Core;
using Timberborn.Modding;
using Timberborn.SingletonSystem;

namespace ConfigurableBeaverWalk
{
    public class ModSettings(
        Timberborn.SettingsSystem.ISettings settings,
        ModSettingsOwnerRegistry modSettingsOwnerRegistry,
        ModRepository modRepository) : ModSettingsOwner(settings, modSettingsOwnerRegistry, modRepository), IUnloadableSingleton
    {
        protected override string ModId => nameof(ConfigurableBeaverWalk);

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

        protected override void OnAfterLoad()
        {
            base.OnAfterLoad();

            changeWalkingSpeed = new(
                true,
                ModSettingDescriptor.Create("Change walking speed")
                    .SetTooltip("If enabled, the walking speed of beavers and bots will be changed."));

            baseWalkingSpeed = new(
                6, 0, 100,
                ModSettingDescriptor.Create("Base walking speed")
                    .SetTooltip("The base walking speed of beavers (Game default: 2.7)")
                    .SetEnableCondition(() => changeWalkingSpeed.Value));

            baseSlowedSpeed = new(
                4, 0, 100,
                ModSettingDescriptor.Create("Base slowed speed")
                    .SetTooltip("The base slowed speed of beavers (Game default: 1.5)")
                    .SetEnableCondition(() => changeWalkingSpeed.Value));

            differentBotSpeed = new(
                false,
                ModSettingDescriptor.Create("Different speed for bots")
                    .SetTooltip("If enabled, bots will have different walking speeds. May have a minor performance impact.")
                    .SetEnableCondition(() => changeWalkingSpeed.Value));

            baseBotWalkingSpeed = new(
                6, 0, 100,
                ModSettingDescriptor.Create("Base walking speed for bots")
                    .SetTooltip("The base walking speed of bots (Game default: 2.7)")
                    .SetEnableCondition(() => changeWalkingSpeed.Value && differentBotSpeed.Value));

            baseBotSlowedSpeed = new(
                4, 0, 100,
                ModSettingDescriptor.Create("Base slowed speed for bots")
                    .SetTooltip("The base slowed speed of bots (Game default: 1.5)")
                    .SetEnableCondition(() => changeWalkingSpeed.Value && differentBotSpeed.Value));

            carryingWeightMultiplier = new(
                1,
                ModSettingDescriptor.Create("Carrying weight multiplier")
                    .SetTooltip("The multiplier for max carrying weight of beavers and bots (Game default: 1)"));

            AddCustomModSetting(changeWalkingSpeed, "beaver_change_walking_speed");
            AddCustomModSetting(baseWalkingSpeed, "beaver_base_walking_speed");
            AddCustomModSetting(baseSlowedSpeed, "beaver_base_slowed_speed");
            AddCustomModSetting(differentBotSpeed, "beaver_different_bot_speed");
            AddCustomModSetting(baseBotWalkingSpeed, "beaver_base_bot_walking_speed");
            AddCustomModSetting(baseBotSlowedSpeed, "beaver_base_bot_slowed_speed");
            AddCustomModSetting(carryingWeightMultiplier, "beaver_carrying_weight_multiplier");

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
}