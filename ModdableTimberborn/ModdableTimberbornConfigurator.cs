namespace ModdableTimberborn;

public class ModdableTimberbornConfigurator : IModdableTimberbornRegistryConfig
{
    
    public void Configure(Configurator configurator, ConfigurationContext context)
    {
        var isGame = context.IsGameContext();
        var isMapEditor = context.IsMapEditorContext();

        if (isGame || isMapEditor)
        {
            configurator
                .BindSingleton<DestructionService>()
            ;
        }
    }

}
