namespace ModdableTimberbornDemo.Features.WorkplaceBuff;

public class WorkplaceBuffConfig : IModdableTimberbornRegistryConfig
{

    public void Configure(Configurator configurator, ConfigurationContext context)
    {
        if (!context.IsGameContext()) { return; }

        configurator
            .BindFragment<DemoWorkplaceBuffFragment>()

            .BindTemplateModule(h => h
                .AddDecorator<Workplace, DemoWorkplaceBuffComponent>()
            )
        ;
    }

}
