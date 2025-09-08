namespace ModdableTimberborn.MechanicalSystem;

public class MechanicalSystemConfigurator : IModdableTimberbornRegistryComponent
{

    public void Configure(Configurator configurator, ConfigurationContext context)
    {
        if (!context.HasFlag(ConfigurationContext.Game)) { return; }

        configurator
            .BindTemplateModule(h => h
                .AddDecorator<MechanicalNodeSpec, ModdableMechanicalNode>()
            )
        ;
    }

}
