namespace ModdableTimberborn.UpdatableEntityStats
{
    public class UpdatableEntityStatsConfig : IModdableTimberbornRegistryConfig
    {
        
        public void Configure(Configurator configurator, ConfigurationContext context)
        {
            if (!context.IsGameContext()) { return; }

            configurator
                .BindSingleton<UpdatableEntityStatService>()
                .BindSingleton<PopulationStatService>()
                .BindTemplateModule(t => t.AddDecorator<TemplateSpec, UpdatableEntityStatComponent>());

            var types = Assembly.GetExecutingAssembly().GetTypes();
            foreach (var t in types)
            {
                if (t.IsClass && !t.IsAbstract && typeof(IUpdatableEntityStat).IsAssignableFrom(t))
                {
                    configurator.MultiBind(typeof(IUpdatableEntityStat), t).AsSingleton();
                }
            }
        }

    }
}

namespace ModdableTimberborn.Registry
{
    public partial class ModdableTimberbornRegistry
    {
        public bool UpdatableEntityStatsUsed { get; private set; }

        public ModdableTimberbornRegistry UseUpdatableEntityStats()
        {
            if (UpdatableEntityStatsUsed) { return this; }

            UpdatableEntityStatsUsed = true;
            AddConfigurator<UpdatableEntityStatsConfig>();

            return this;
        }
    }
}
