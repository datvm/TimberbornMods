namespace ModdableTimberborn.ModdableWaterSource
{
    public class WaterSourceConfig : IModdableTimberbornRegistryWithPatchConfig
    {
        public static readonly WaterSourceConfig Instance = new();
        public const string PatchCategoryName = $"{nameof(ModdableTimberborn)}.{nameof(WaterSource)}";

        public string? PatchCategory => PatchCategoryName;
        public ConfigurationContext AvailableContexts => ConfigurationContext.Game;

        public void Configure(Configurator configurator, ConfigurationContext context)
        {
            configurator
                .BindSingleton<ModdableWaterSourceService>()
                .BindTemplateModule(h => h
                    .AddDecorator<WaterSource, ModdableWaterSourceComponent>()
                );
        }
    }

}

namespace ModdableTimberborn.Registry
{
    public partial class ModdableTimberbornRegistry
    {
        public bool WaterSourceUsed { get; private set; }
        public ModdableTimberbornRegistry UseWaterSource()
        {
            if (WaterSourceUsed) { return this; }

            WaterSourceUsed = true;
            AddConfigurator(WaterSourceConfig.Instance);

            UseEntityTracker()
                .TryTrack<ModdableWaterSourceComponent>();

            return this;
        }
    }
}