namespace MoreHttpApiAutomations;

public class MMoreHttpApiAutomationsConfigs : BaseModdableTimberbornConfiguration
{
    public override ConfigurationContext AvailableContexts => ConfigurationContext.Game;

    public override void Configure(Configurator configurator, ConfigurationContext context)
    {
        configurator.BindMoreHttpApiHandlers();
    }

}
