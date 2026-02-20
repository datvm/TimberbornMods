namespace BuildingBlueprintsSteamWorkshop;

public class MConfigs : BaseModdableTimberbornAttributeConfiguration
{

    public override ConfigurationContext AvailableContexts { get; } = ConfigurationContext.Game;

    public override void Configure(Configurator configurator, ConfigurationContext context)
    {
        base.Configure(configurator, context);

        // Add this to the Game context
        if (!configurator.IsBound<SteamWorkshopUploadPanel>())
        {
            new SteamWorkshopUIConfigurator().Configure(configurator._containerDefinition);
        }
    }

}
