
namespace ModdableTimberborn;

public class ModdableTimberbornConfigurator : IModdableTimberbornRegistryConfig
{
    ConfigurationContext IModdableTimberbornRegistryConfig.AvailableContexts => ConfigurationContext.NonBootstrapper;

    public void Configure(Configurator configurator, ConfigurationContext context)
    {
        configurator.BindAttributes(context);
    }

}
