
namespace ModdableTimberbornDemo.Features.EnterableBuff;

public class EnterableBuffConfig : IModdableTimberbornRegistryConfig
{
    public void Configure(Configurator configurator, ConfigurationContext context)
    {
        if (!context.IsGameContext()) { return; }

        configurator
            .BindFragment<DemoEnterableBuffFragment>()
            .BindTemplateModule(h => h
                .AddDecorator<Enterable, DemoEnterableBuffComponent>()
            )
        ;
    }
}
