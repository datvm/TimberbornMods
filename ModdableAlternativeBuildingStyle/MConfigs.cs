namespace ModdableAlternativeBuildingStyle;

public class MConfigs : BaseModdableTimberbornAttributeConfiguration
{
    public override ConfigurationContext AvailableContexts => ConfigurationContext.Game;

    protected override void ConfigureRegistry(ModdableTimberbornRegistry registry)
        => registry.UseBuildingSettings();

}
