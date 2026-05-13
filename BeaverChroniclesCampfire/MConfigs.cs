global using BeaverChroniclesCampfire.Events;
global using BeaverChroniclesCampfire.Helpers;
global using ModdableTimberborn.EntityTracker;

namespace BeaverChroniclesCampfire;

public class MBCCampfireConfigs : BaseModdableTimberbornAttributeConfiguration
{
    public override ConfigurationContext AvailableContexts => ConfigurationContext.Game;

    public override void StartMod(IModEnvironment modEnvironment)
    {
        base.StartMod(modEnvironment);

        ModdableTimberbornRegistry.Instance
            .TryTrack<FloodableObject>();
    }

    public override void Configure(Configurator configurator, ConfigurationContext context)
    {
        base.Configure(configurator, context);
        configurator.BindAllEvents();
    }
}
