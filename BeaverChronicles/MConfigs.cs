namespace BeaverChronicles;

public class MBeaverChroniclesConfigs : BaseModdableTimberbornAttributeConfiguration
{

    public override ConfigurationContext AvailableContexts => ConfigurationContext.Game;

    public override void StartMod(IModEnvironment modEnvironment)
    {
        base.StartMod(modEnvironment);

        ModdableTimberbornRegistry.Instance
            .UseEntityTracker()
            .TryTrack<Stockpile>()
            .TryTrack<BlockObject>()
            .TryTrack<Bot>()
        ;
    }

    public override void Configure(Configurator configurator, ConfigurationContext context)
    {
        base.Configure(configurator, context);

        configurator.BindAllEvents();
    }

}
