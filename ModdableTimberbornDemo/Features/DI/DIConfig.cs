
namespace ModdableTimberbornDemo.Features.DI;

public class DIConfig : IModdableTimberbornRegistryConfig
{
    public void Configure(Configurator configurator, ConfigurationContext context)
    {
        if (!context.IsGameContext()) { return; }

        configurator
            .BindSingleton<DemoFactionServiceRunner>()
        ;
    }
}
