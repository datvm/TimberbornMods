
namespace ModdableTimberborn.BonusSystem
{
    public class BonusSystemConfig : IModdableTimberbornRegistryConfig
    {

        public void Configure(Configurator configurator, ConfigurationContext context)
        {
            if (!context.IsGameplayContext()) { return; }

            var registry = ModdableTimberbornRegistry.Instance;
            configurator.BindTemplateModule(h =>
            {
                if (registry.BonusTrackerUsed)
                {
                    h.AddDecorator<BonusManager, BonusTrackerComponent>();
                }

                if (registry.PersistentBonusTrackerUsed)
                {
                    h.AddDecorator<BonusManager, PersistentBonusTrackerComponent>();
                }

                return h;
            });
        }

    }
}

namespace ModdableTimberborn.Registry
{

    public partial class ModdableTimberbornRegistry
    {

        public bool BonusTrackerUsed { get; private set; }
        public bool PersistentBonusTrackerUsed { get; private set; }

        public ModdableTimberbornRegistry UseBonusTracker() => UseBonusTracker(false);
        public ModdableTimberbornRegistry UsePersistentBonusTracker() => UseBonusTracker(true);

        public ModdableTimberbornRegistry UseBonusTracker(bool persistent)
        {
            if (!BonusTrackerUsed && !PersistentBonusTrackerUsed)
            {
                AddConfigurator<BonusSystemConfig>();
            }

            if (persistent)
            {
                PersistentBonusTrackerUsed = true;
            }
            else
            {
                BonusTrackerUsed = true;
            }

            return this;
        }
    }

}