
namespace ModdableTimberborn;

public class ModdableTimberbornConfigurator : IModdableTimberbornRegistryConfig
{
    ConfigurationContext IModdableTimberbornRegistryConfig.AvailableContexts { get; } = ConfigurationContext.Game | ConfigurationContext.MapEditor;

    public void Configure(Configurator configurator, ConfigurationContext context)
    {
        configurator
            .BindSingleton<DestructionService>()
            .BindSingleton<PersistentGameModeService>()
        ;
    }

}
