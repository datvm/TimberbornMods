namespace ModdableTimberborn;

public class ModdableTimberbornConfigurator : IModdableTimberbornRegistryComponent
{
    
    public void Configure(Configurator configurator, ConfigurationContext context)
    {
        var isGame = context.HasFlag(ConfigurationContext.Game);
        var isMapEditor = context.HasFlag(ConfigurationContext.MapEditor);

        if (isGame || isMapEditor)
        {
            configurator
                .BindSingleton<DestructionService>()
            ;
        }
    }

}
