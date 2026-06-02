namespace MoreHttpApi;

public class MMoreHttpApiConfigs : BaseModdableTimberbornAttributeConfiguration, IModdableTimberbornRegistryWithPatchConfig
{
    public override ConfigurationContext AvailableContexts => ConfigurationContext.All;
    public string? PatchCategory { get; }

    public override void StartMod(IModEnvironment modEnvironment)
    {
        base.StartMod(modEnvironment);

        ModdableTimberbornRegistry.Instance
            .UseBuildingSettings();
    }

    public override void Configure(Configurator configurator, ConfigurationContext context)
    {
        base.Configure(configurator, context);

        if (!context.IsGameContext()) { return; }

        configurator.BindMoreHttpApiHandlers();
    }

}