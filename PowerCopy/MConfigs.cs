namespace PowerCopy;

public class MConfigs : BaseModdableTimberbornAttributeConfiguration
{
    public override ConfigurationContext AvailableContexts { get; } = ConfigurationContext.NonMenu;

    public override void StartMod(IModEnvironment modEnvironment)
    {
        base.StartMod(modEnvironment);

        ModdableTimberbornRegistry.Instance.UseBuildingSettings();
    }

}
