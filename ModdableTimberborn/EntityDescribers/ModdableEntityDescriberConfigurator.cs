namespace ModdableTimberborn.EntityDescribers;

public class ModdableEntityDescriberConfigurator : IModdableTimberbornRegistryConfig
{

    ConfigurationContext IModdableTimberbornRegistryConfig.AvailableContexts { get; } = ConfigurationContext.Game;

    public void Configure(Configurator configurator, ConfigurationContext context)
    {
        configurator.BindOrderedFragment<EntityEffectDescriberFragment>();
    }

}
