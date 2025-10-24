global using AlternativeRecipesSPs.Services;

namespace AlternativeRecipesSPs;

public class MConfigs : BaseModdableTimberbornConfiguration
{

    public override void Configure(Configurator configurator, ConfigurationContext context)
    {
        if (!context.IsGameContext()) { return; }

        configurator
            .BindSingleton<ProjectRecipeDescriber>()
            .BindSingleton<ProjectRecipeUnlocker>()
            .MultiBindSingleton<IDefaultRecipeLocker, ProjectRecipeLocker>()
        ;
    }

}
