namespace ModdableTimberborn.EntityDescribers;

public class ModdableEntityDescriberConfigurator : IModdableTimberbornRegistryComponent
{

    public void Configure(Configurator configurator, ConfigurationContext context)
    {
        if (!context.HasFlag(ConfigurationContext.Game)) { return; }

        configurator.BindFragment<EntityDescriberFragment>();
    }

}
