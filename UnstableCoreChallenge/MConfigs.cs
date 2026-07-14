namespace UnstableCoreChallenge;

public class MConfigs : BaseModdableTimberbornAttributeConfiguration, IModdableTimberbornRegistryWithPatchConfig
{
    public override ConfigurationContext AvailableContexts => ConfigurationContext.MainMenu | ConfigurationContext.Game;
    public string? PatchCategory { get; }

    public override void StartMod(IModEnvironment modEnvironment)
    {
        base.StartMod(modEnvironment);

        ModdableTimberbornRegistry.Instance
            .UseGameStats()
            .UseEntityTracker()
            .TryTrack<UnstableCoreStabilizer>();
    }

}
