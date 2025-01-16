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

        public static float BaseWalkingSpeed { get; private set; }
        public static float BaseSlowedSpeed { get; private set; }

        public static bool DifferentForBots { get; private set; }
        public static float BaseBotWalkingSpeed { get; private set; }
        public static float BaseBotSlowedSpeed { get; private set; }

        RangeIntModSetting? baseWalkingSpeed, baseSlowedSpeed, baseBotWalkingSpeed, baseBotSlowedSpeed;
        ModSetting<bool>? differentBotSpeed;

        protected override void OnAfterLoad()
        {
            base.OnAfterLoad();

            baseWalkingSpeed = new RangeIntModSetting(
                6, 0, 100,
                ModSettingDescriptor.Create("Base walking speed")
                    .SetTooltip("The base walking speed of beavers (Game default: 2.7)"));

            baseSlowedSpeed = new RangeIntModSetting(
                4, 0, 100,
                ModSettingDescriptor.Create("Base slowed speed")
                    .SetTooltip("The base slowed speed of beavers (Game default: 1.5)"));

            differentBotSpeed = new ModSetting<bool>(
                false,
                ModSettingDescriptor.Create("Different speed for bots")
                    .SetTooltip("If enabled, bots will have different walking speeds. May have a minor performance impact."));

            baseBotWalkingSpeed = new RangeIntModSetting(
                6, 0, 100,
                ModSettingDescriptor.Create("Base walking speed for bots")
                    .SetTooltip("The base walking speed of bots (Game default: 2.7)")
                    .SetEnableCondition(() => differentBotSpeed?.Value == true));

            baseBotSlowedSpeed = new RangeIntModSetting(
                4, 0, 100,
                ModSettingDescriptor.Create("Base slowed speed for bots")
                    .SetTooltip("The base slowed speed of bots (Game default: 1.5)")
                    .SetEnableCondition(() => differentBotSpeed?.Value == true));

            AddCustomModSetting(baseWalkingSpeed, "beaver_base_walking_speed");
            AddCustomModSetting(baseSlowedSpeed, "beaver_base_slowed_speed");
            AddCustomModSetting(differentBotSpeed, "beaver_different_bot_speed");
            AddCustomModSetting(baseBotWalkingSpeed, "beaver_base_bot_walking_speed");
            AddCustomModSetting(baseBotSlowedSpeed, "beaver_base_bot_slowed_speed");

            UpdateValues();
        }

        void UpdateValues()
        {
            BaseWalkingSpeed = baseWalkingSpeed?.Value ?? 0;
            BaseSlowedSpeed = baseSlowedSpeed?.Value ?? 0;
            DifferentForBots = differentBotSpeed?.Value == true;
            BaseBotWalkingSpeed = baseBotWalkingSpeed?.Value ?? 0;
            BaseBotSlowedSpeed = baseBotSlowedSpeed?.Value ?? 0;
        }

        public void Unload()
        {
            UpdateValues();
        }
    }
}