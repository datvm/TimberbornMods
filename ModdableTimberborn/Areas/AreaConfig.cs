namespace ModdableTimberborn.Areas
{
    public class AreaConfig : IModdableTimberbornRegistryConfig
    {
        ConfigurationContext IModdableTimberbornRegistryConfig.AvailableContexts => ConfigurationContext.Game;

        public void Configure(Configurator configurator, ConfigurationContext context)
        {
            configurator
                .BindSingleton<AreaSegmentService>()
                .BindSingleton<CharacterAreaTrackerService>()
                .BindTemplateModule(h => h
                    .AddDecorator<Character, CharacterPositionTracker>()
                )
            ;
        }

    }
}

namespace ModdableTimberborn.Registry
{

    public partial class ModdableTimberbornRegistry
    {
        public bool AreaApisUsed { get; private set; }

        public ModdableTimberbornRegistry UseAreaApis()
        {
            if (AreaApisUsed) { return this; }

            AreaApisUsed = true;
            AddConfigurator<AreaConfig>();
            UseEntityTracker();
            TryTrack<CharacterPositionTracker>();

            return this;
        }
    }

}