global using ModdableTimberborn.DependencyInjection;

global using ConfigurablePlants.Services;

namespace ConfigurablePlants;

public class ConfigurablePlantsConfigs : BaseModdableTimberbornConfigurationWithHarmony, IWithDIConfig
{
    public override ConfigurationContext AvailableContexts { get; } = ConfigurationContext.MainMenu | ConfigurationContext.Game;

    public override void Configure(Configurator configurator, ConfigurationContext context)
    {
        configurator.BindSingleton<MSettings>();

        if (!context.IsGameContext()) { return; }

        configurator
            .BindSingleton<NoGroundPlantTerrainService>()
            .BindTemplateModifier<PlantTemplateModifier>()
        ;
    }

}
