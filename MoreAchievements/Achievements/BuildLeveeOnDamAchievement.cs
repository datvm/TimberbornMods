namespace MoreAchievements.Achievements;

[MultiBind(typeof(Achievement))]
public class BuildLeveeOnDamAchievement(
    EventBus eb,
    IBlockService blocks
) : EbAchievementBase(eb)
{
    public const string AchId = "LV.MA.BuildLeveeOnDam";
    public override string Id => AchId;

    const string LeveeTemplate = "Levee";
    const string DamTemplate = "Dam";

    [OnEvent]
    public void OnNewBuilding(EnteredFinishedStateEvent e)
    {
        if (e.BlockObject.GetTemplateName() != LeveeTemplate) { return; }

        var coord = e.BlockObject.Coordinates;
        coord = coord with { z = coord.z - 1, };

        var objs = blocks.GetObjectsAt(coord);
        foreach (var obj in objs)
        {
            if (obj.GetTemplateName() == DamTemplate)
            {
                Unlock();
                break;
            }
        }
    }

}
