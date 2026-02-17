namespace ModdableTimberborn.BuildingSettings
{
    public class BuildingSettingsConfig : IModdableTimberbornRegistryConfig
    {
        public static readonly BuildingSettingsConfig Instance = new();

        ConfigurationContext IModdableTimberbornRegistryConfig.AvailableContexts => ConfigurationContext.NonMenu;

        public void Configure(Configurator configurator, ConfigurationContext context)
        {
            configurator
                .BindSingleton<BuildingSettingsResolver>()
            ;

            foreach (var t in typeof(BuildingSettingsConfig).Assembly.GetTypes())
            {
                if (!t.IsAbstract && t.IsClass && typeof(IBuildingSettings).IsAssignableFrom(t))
                {
                    configurator.MultiBind(typeof(IBuildingSettings), t).AsSingleton();
                }
            }
        }
    }
}

namespace ModdableTimberborn.Registry
{
    public partial class ModdableTimberbornRegistry
    {
        public bool BuildingSettingsUsed { get; private set; }
        public ModdableTimberbornRegistry UseBuildingSettings()
        {
            if (BuildingSettingsUsed) { return this; }

            BuildingSettingsUsed = true;
            AddConfigurator(BuildingSettingsConfig.Instance);

            return this;
        }
    }
}