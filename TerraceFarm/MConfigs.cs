namespace TerraceFarm;

public class MConfigs : BaseModdableTimberbornAttributeConfiguration, IWithDIConfig
{
    public override ConfigurationContext AvailableContexts => ConfigurationContext.Game;

    public override void StartMod(IModEnvironment modEnvironment)
    {
        base.StartMod(modEnvironment);

        ModdableTimberbornRegistry.Instance
            .UseEntityTracker()
            .TryTrack<TerraceComponent>()
            .TryTrack<Growable>();
    }

}
