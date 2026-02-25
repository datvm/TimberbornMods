
namespace HueAndTurnX;

public class HueAndTurnConfigs : BaseModdableTimberbornAttributeConfiguration
{
    public override ConfigurationContext AvailableContexts => ConfigurationContext.NonMenu;

    public override void Configure(Configurator configurator, ConfigurationContext context)
    {
        base.Configure(configurator, context);

        configurator.BindFragment<HueTurnFragment>();
    }

}
