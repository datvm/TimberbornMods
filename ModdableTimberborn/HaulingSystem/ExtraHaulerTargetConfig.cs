namespace ModdableTimberborn.HaulingSystem
{
    public class ExtraHaulerTargetConfig : IModdableTimberbornRegistryWithPatchConfig
    {
        public const string PatchCategoryName = $"{nameof(ModdableTimberborn)}.{nameof(HaulingSystem)}";

        public static readonly ExtraHaulerTargetConfig Instance = new();

        ConfigurationContext IModdableTimberbornRegistryConfig.AvailableContexts => ConfigurationContext.Game;

        public string PatchCategory { get; } = PatchCategoryName;

        ExtraHaulerTargetConfig() { }

        public void Configure(Configurator configurator, ConfigurationContext context)
        {
            // Don't bind HaulingTargetHelper here, it's intentionally bound no matter if this is used or not since it is just a helper.

            configurator
                .BindSingleton<ExtraHaulerTargetService>()
            ;
        }
    }
}

namespace ModdableTimberborn.Registry
{
    public partial class ModdableTimberbornRegistry
    {
        public bool ExtraHaulerTargetsUsed { get; private set; }

        public ModdableTimberbornRegistry UseExtraHaulerTargets()
        {
            if (ExtraHaulerTargetsUsed)
            {
                return this;
            }

            ExtraHaulerTargetsUsed = true;
            AddConfigurator(ExtraHaulerTargetConfig.Instance);
            return this;
        }
    }
}
