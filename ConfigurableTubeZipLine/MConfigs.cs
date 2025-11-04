namespace ConfigurableTubeZipLine;

public class MConfigs : BaseModdableTimberbornConfigurationWithHarmony
{
    public override ConfigurationContext AvailableContexts { get; } = ConfigurationContext.MainMenu | ConfigurationContext.Game;

    public override void StartMod(IModEnvironment modEnvironment)
    {
        base.StartMod(modEnvironment);

        ModdableTimberbornRegistry.Instance
            .UseDependencyInjection()
        ;
    }

    public override void Configure(Configurator configurator, ConfigurationContext context)
    {
        configurator.BindSingleton<MSettings>();

        if (!context.IsGameContext()) { return; }

        configurator
            // Zipline
            .BindTemplateModifier<ZiplineTemplateModifier>()
            .BindSpecModifier<ZiplineCableInclinationModifier>()
            .BindSpecModifier<ZiplineSpeedModifier>()

            // Tubeway
            .BindTemplateModifier<TubewayTemplateModifier>()
            
            // Buildings for others
            .BindSpecModifier<FactionSpecModifier>()
            .BindSpecModifier<TemplateCollectionSpecModifier>()
        ;
    }
}