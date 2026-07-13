namespace UnstableCoreChallenge;

public class MConfigs : BaseModdableTimberbornAttributeConfiguration
{
    public override ConfigurationContext AvailableContexts => ConfigurationContext.MainMenu | ConfigurationContext.Game;

    public override void StartMod(IModEnvironment modEnvironment)
    {
        base.StartMod(modEnvironment);

        ModdableTimberbornRegistry.Instance
            .UseGameStats()
            .UseEntityTracker()
            .TryTrack<UnstableCoreStabilizer>();
    }

}
