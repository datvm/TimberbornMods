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
            configurator.BindSingleton<ExtraHaulerTargetService>();
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
