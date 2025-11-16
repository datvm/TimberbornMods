global using ConfigurableDaisugiForestry.Services;
global using ConfigurableDaisugiForestry.UI;
global using ModdableTimberborn.DependencyInjection;

namespace ConfigurableDaisugiForestry;

public class ConfigurableDaisugiForestryConfigs : BaseModdableTimberbornConfiguration
{
    public override ConfigurationContext AvailableContexts { get; } = ConfigurationContext.All & ~ConfigurationContext.Bootstrapper;

    public override void StartMod(IModEnvironment modEnvironment)
    {
        base.StartMod(modEnvironment);

        ModdableTimberbornRegistry.Instance
            .UseDependencyInjection();
    }

    public override void Configure(Configurator configurator, ConfigurationContext context)
    {
        configurator
            .BindSingleton<MSettings>()
            .MultiBindAndBindSingleton<IModSettingElementFactory, MSettingDifficultyFactory>()
            .BindTransient<MSettingDifficultyElement>();

        if (!context.IsGameContext()) { return; }

        configurator
            .BindTemplateModifier<DaisugiModifier>()
        ;
    }
}
