namespace BeaverChronicles;

public class MBeaverChroniclesConfigs : BaseModdableTimberbornAttributeConfiguration
{

    public override ConfigurationContext AvailableContexts => ConfigurationContext.Game;

    public override void StartMod(IModEnvironment modEnvironment)
    {
        base.StartMod(modEnvironment);

        ModdableTimberbornRegistry.Instance
            .UseBonusTracker()
            .UseEntityTracker()
            .UseGameStats()
            .UseAreaApis()
            .UseWaterSource()
            .TryTrack<Stockpile>()
            .TryTrack<BlockObject>()
            .TryTrack<BlockObjectBound>()
            .TryTrack<Bot>()
            .TryTrack<Beaver>()
        ;
    }

}
