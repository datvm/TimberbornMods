namespace MoreAchievements.Achievements;

[MultiBind(typeof(Achievement))]
public class LaunchRepopulatorWithBotsAchievement(
    AchievementWonderService service,
    BeaverPopulation beaverPopulation
) : BaseWonderLaunchAchievement(service)
{
    public const string TemplateName = "EarthRepopulator.IronTeeth";

    public static string AchId = "LV.MA.LaunchRepopulatorWithBots";
    public override string Id => AchId;

    public override bool CanBeAchieved => service.WonderTemplates.ContainsKey(TemplateName);

    public override bool ShouldUnlock(Wonder launchedWonder)
    {
        var template = launchedWonder.GetTemplateName();

        return template == TemplateName && beaverPopulation.NumberOfBeavers == 0;
    }

}
