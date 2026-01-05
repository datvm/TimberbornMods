global using AlternativeRecipesSPs.Services;

namespace AlternativeRecipesSPs;

public class MConfigs : BaseModdableTimberbornConfiguration
{
    public override ConfigurationContext AvailableContexts { get; } = ConfigurationContext.Game;

    public override void Configure(Configurator configurator, ConfigurationContext context)
    {
        configurator
            .BindSingleton<ProjectRecipeDescriber>()
            .BindSingleton<ProjectRecipeUnlocker>()
        ;
    }

}
