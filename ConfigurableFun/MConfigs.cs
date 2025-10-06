global using ConfigurableFun.Services;
global using ModdableTimberborn.DependencyInjection;

namespace ConfigurableFun;

public class MConfig : IModdableTimberbornRegistryConfig, IModStarter
{

    void IModStarter.StartMod(IModEnvironment modEnvironment)
    {
        ModdableTimberbornRegistry.Instance
            .UseDependencyInjection()
            .AddConfigurator(this);
    }

    public void Configure(Configurator configurator, ConfigurationContext context)
    {
        if (context.IsBootstrapperContext()) { return; }
        configurator.BindSingleton<MSettings>();

        if (!context.IsGameContext()) { return; }        
        configurator.MultiBindSingleton<IPrefabModifier, PrefabModifier>();
    }
}
