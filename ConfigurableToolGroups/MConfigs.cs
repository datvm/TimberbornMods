namespace ConfigurableToolGroups;

public class ConfigurableToolGroupsConfig : BaseModdableTimberbornConfigurationWithHarmony, IWithDIConfig
{
    public override ConfigurationContext AvailableContexts { get; } = ConfigurationContext.Game | ConfigurationContext.MapEditor;

    public override void Configure(Configurator configurator, ConfigurationContext context)
    {
        configurator
            .BindTemplateTailRunner<ModdableToolGroupSpecService>(true)
        ;
    }

}
