
namespace ConfigurableFaction;

public class MConfig : BaseModdableTimberbornAttributeConfiguration
{
    public override ConfigurationContext AvailableContexts => ConfigurationContext.Bootstrapper | ConfigurationContext.MainMenu | ConfigurationContext.Game;

    public override void Configure(Configurator configurator, ConfigurationContext context)
    {
        base.Configure(configurator, context);

        if (context.IsMenuContext())
        {
            configurator.TryBindingSpriteOperations();
            configurator.TryBindingSystemFileDialogService();
        }
    }

}