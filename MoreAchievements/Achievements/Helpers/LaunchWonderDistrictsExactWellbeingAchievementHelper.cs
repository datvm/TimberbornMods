namespace MoreAchievements.Achievements.Helpers;

[MultiBind(typeof(BaseAchievementHelper))]
public class LaunchWonderDistrictsExactWellbeingAchievementHelper(DialogService diag, ILoc t, AchievementWonderService service)
    : BaseMessageBoxAchievementHelper<LaunchWonderDistrictsExactWellbeingAchievement>("LV.MA.LaunchWonderDistrictsExactWellbeing", 1, diag, t)
{

    protected override object[] GetParameters(int step)
    {
        var a = Achievement;
        var wellbeings = a.GetDistrictWellbeings().ToArray();

        var count = wellbeings.Length;
        var counter = count.ToString().Color(
            (count == LaunchWonderDistrictsExactWellbeingAchievement.DistrictCount1 || count == LaunchWonderDistrictsExactWellbeingAchievement.DistrictCount2)
            ? TimberbornTextColor.Green : TimberbornTextColor.Red);

        var list = string.Join(Environment.NewLine, wellbeings.Select(wd =>
        {
            var (dc, w) = wd;
            return $"• {dc.DistrictName}: {w}".Color(
                w == LaunchWonderDistrictsExactWellbeingAchievement.RequiredWellbeing
                ? TimberbornTextColor.Green
                : TimberbornTextColor.Red);
        }));

        return [counter, list];
    }

}
