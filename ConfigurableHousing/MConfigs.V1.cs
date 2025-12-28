global using ConfigurableHousing.Patches.V1;
global using ModdableTimberborn.DependencyInjection;

namespace ConfigurableHousing;

public class ConfigurableHousingConfigs : BaseModdableTimberbornConfiguration, IWithDIConfig
{
    public override ConfigurationContext AvailableContexts { get; } = ConfigurationContext.Game | ConfigurationContext.MainMenu;

    public override void Configure(Configurator configurator, ConfigurationContext context)
    {
        configurator.BindSingleton<MSettings>();

        if (!context.IsGameContext()) { return; }
        
        configurator
            .BindTemplateModifier<HousingModifier>()

            .MultiBindSingleton<IMaterialCollectionIdsProvider, MaterialMerger>()
            .MultiBindSingleton<ITemplateCollectionIdProvider, DwellingAdder>()
        ;
    }

}
