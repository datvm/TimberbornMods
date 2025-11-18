global using ConfigurableExplosives.Components;
global using ConfigurableExplosives.Services;
global using ConfigurableExplosives.UI;
global using ModdableTimberborn.DependencyInjection;

namespace ConfigurableExplosives;

public class ConfigurableExplosivesConfigs : BaseModdableTimberbornConfigurationWithHarmony, IWithDIConfig
{
    public override ConfigurationContext AvailableContexts { get; } = ConfigurationContext.Game | ConfigurationContext.MainMenu;

    public override void Configure(Configurator configurator, ConfigurationContext context)
    {
        configurator.BindSingleton<MSettings>();

        if (!context.IsGameContext()) { return; }

        configurator
            .BindTemplateModifier<DynamiteTemplateModifier>()

            .BindFragment<ConfigurableDynamiteFragment>()

            .BindTemplateModule(h => h
                .AddDecorator<Dynamite, ConfigurableDynamiteComponent>()
            )
        ;
    }
}
