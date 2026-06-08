namespace ModdableTimberborn.GameStats
{
    public class GameStatsConfig : IModdableTimberbornRegistryConfig
    {

        ConfigurationContext IModdableTimberbornRegistryConfig.AvailableContexts => ConfigurationContext.Game;

        public void Configure(Configurator configurator, ConfigurationContext context)
        {
            foreach (var t in typeof(GameStatsConfig).Assembly.GetTypes())
            {
                if (!t.IsAbstract && typeof(IGameStatProvider).IsAssignableFrom(t))
                {
                    configurator.MultiBind(typeof(IGameStatProvider), t).AsSingleton();
                }
            }

            configurator
                .BindSingleton<GameStatService>()
            ;
        }
    }
}

namespace ModdableTimberborn.Registry
{

    public partial class ModdableTimberbornRegistry
    {
        public bool GameStatsUsed { get; private set; }

        public ModdableTimberbornRegistry UseGameStats()
        {
            if (GameStatsUsed) { return this; }

            GameStatsUsed = true;
            AddConfigurator<GameStatsConfig>();

            return this;
        }
    }

}