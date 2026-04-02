namespace MoreAchievements.Achievements;

[MultiBind(typeof(Achievement))]
public class TelescopeAtBottomAchievement(TemplateNameMapper templateNameMapper, EventBus eb) : EbAchievementBase(eb)
{
    public static string AchId = "LV.MA.TelescopeAtBottom";
    public const string TemplateName = "Observatory.Folktails";

    public override string Id => AchId;

    public override void EnableInternal()
    {
        if (!templateNameMapper.TryGetTemplate(TemplateName, out _))
        {
            Disable();
            return;
        }

        base.EnableInternal();
    }

    [OnEvent]
    public void OnBuildingFinished(EnteredFinishedStateEvent e)
    {
        if (e.BlockObject.GetTemplateName() == TemplateName && e.BlockObject.Coordinates.z == 0)
        {
            Unlock();
        }
    }

}
