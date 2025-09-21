global using BeavVsMachine.Components;
global using ModdableTimberborn.BonusSystem;

namespace BeavVsMachine;

public class MConfig : IModdableTimberbornRegistryConfig
{
    public void Configure(Configurator configurator, ConfigurationContext context)
    {
        if (!context.IsGameContext()) { return; }

        configurator
            .BindTemplateModule(h => h
                .AddDecorator<BeaverSpec, BeaverExpComponent>()
                .AddDecorator<BeaverSpec, BeaverFitnessComponent>()
            )
        ;
    }
}

public class MStarter : IModStarter
{

    void IModStarter.StartMod(IModEnvironment modEnvironment)
    {
        ModdableTimberbornRegistry.Instance
            .UseBonusTracker()
            .AddConfigurator<MConfig>();
    }

}
