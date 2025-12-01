namespace TImprove4Achievements.Services.Helpers;

public class LargeTubewayNetworkAchievementHelper(DialogService diag, ILoc t)
    : BaseMessageBoxAchievementHelper<LargeTubewayNetworkAchievement>("LV.T4A.LargeTubewayNetwork", 1, diag, t)
{
    protected override object[] GetParameters(int step)
    {
        return [
            Achievement._stationCount,
            LargeTubewayNetworkAchievement.StationsRequired,
            Achievement._tubewayCount,
            LargeTubewayNetworkAchievement.TubewaysRequired,
        ];
    }

}