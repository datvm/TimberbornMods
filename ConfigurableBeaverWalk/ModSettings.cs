using ModSettings.Common;
using ModSettings.Core;
using Timberborn.Modding;
using Timberborn.SingletonSystem;

namespace ConfigurableBeaverWalk
{
    public class ModSettings : ModSettingsOwner, IUnloadableSingleton
    {
        public ModSettings(
            Timberborn.SettingsSystem.ISettings settings,
            ModSettingsOwnerRegistry modSettingsOwnerRegistry,
            ModRepository modRepository) 
            : base(settings, modSettingsOwnerRegistry, modRepository)
        {
        }

        protected override string ModId => nameof(ConfigurableBeaverWalk);

        public static float BaseWalkingSpeed { get; private set; }
        public static float BaseSlowedSpeed { get; private set; }

        protected override void OnAfterLoad()
        {
            base.OnAfterLoad();

            {
                var desc = ModSettingDescriptor.Create("Base walking speed")
                    .SetTooltip("The base walking speed of beavers (Game default: 2.7)");

                var settings = new RangeIntModSetting(6, 0, 100, desc);
                AddCustomModSetting(settings, "beaver_base_walking_speed");

                BaseWalkingSpeed = settings.Value;
            }

            {
                var desc = ModSettingDescriptor.Create("Base slowed speed")
                    .SetTooltip("The base slowed speed of beavers (Game default: 1.5)");

                var settings = new RangeIntModSetting(4, 0, 100, desc);
                AddCustomModSetting(settings, "beaver_base_slowed_speed");

                BaseSlowedSpeed = settings.Value;
            }

        }

        public void Unload()
        {
            BaseWalkingSpeed = 0;
            BaseSlowedSpeed = 0;
        }
    }
}