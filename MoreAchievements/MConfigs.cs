global using MoreAchievements.Achievements;
global using ModdableTimberborn.EntityTracker;
global using MoreAchievements.Components;
global using MoreAchievements.Services;

namespace MoreAchievements;

public class MoreAchievementsConfigs : BaseModdableTimberbornAttributeConfiguration
{

    public override ConfigurationContext AvailableContexts { get; } = ConfigurationContext.Game;

    public override void StartMod(IModEnvironment modEnvironment)
    {
        base.StartMod(modEnvironment);

        ModdableTimberbornRegistry.Instance
            .UseEntityTracker()
            .TryTrack<Wonder>()
            .TryTrack<Stockpile>()
            .TryTrack<Manufactory>()
            .TryTrack<FloodableObject>()
            .TryTrack<NumbercruncherSubmergeComponent>()
        ;
    }

}