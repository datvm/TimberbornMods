
namespace ConfigurableFaction;

public class MConfig : BaseModdableTimberbornAttributeConfiguration, IWithDIConfig, IModdableTimberbornRegistryWithPatchConfig
{
    public override ConfigurationContext AvailableContexts => ConfigurationContext.Bootstrapper | ConfigurationContext.MainMenu | ConfigurationContext.Game;

    public string? PatchCategory { get; }

    public override void Configure(Configurator configurator, ConfigurationContext context)
    {
        base.Configure(configurator, context);

        switch (context)
        {
            case ConfigurationContext.MainMenu:
                configurator.TryBindingSpriteOperations();
                configurator.TryBindingSystemFileDialogService();
                break;
            case ConfigurationContext.Bootstrapper:
                configurator.Bind<UserSettingsService>().AsSingleton().AsExported();
                break;
        }
    }

}