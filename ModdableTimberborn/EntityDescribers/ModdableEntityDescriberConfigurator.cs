namespace ModdableTimberborn.EntityDescribers;

public class ModdableEntityDescriberConfigurator : IModdableTimberbornRegistryConfig
{

    public void Configure(Configurator configurator, ConfigurationContext context)
    {
        if (!context.IsGameContext()) { return; }

        configurator.BindOrderedFragment<EntityEffectDescriberFragment>();
    }

}
