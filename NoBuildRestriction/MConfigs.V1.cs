global using ModdableTimberborn.DependencyInjection;

global using NoBuildRestriction.Services;
global using NoBuildRestriction.Patches;

namespace NoBuildRestriction;

public class NoBuildRestrictionConfig : BaseModdableTimberbornConfigurationWithHarmony, IWithDIConfig
{
    public override ConfigurationContext AvailableContexts { get; } = ConfigurationContext.MainMenu | ConfigurationContext.Game;

    public override void Configure(Configurator configurator, ConfigurationContext context)
    {
        configurator.BindSingleton<MSettings>();

        if (!context.IsGameContext()) { return; }

        configurator
            .BindTemplateModifier<RestrictionTemplateModifier>()
        ;
    }
}
