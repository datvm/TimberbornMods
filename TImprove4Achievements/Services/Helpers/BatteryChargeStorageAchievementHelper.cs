namespace TImprove4Achievements.Services.Helpers;

public class BatteryChargeStorageAchievementHelper(DialogService diag, ILoc t)
    : BaseMessageBoxAchievementHelper<BatteryChargeStorageAchievement>("LV.T4A.BatteryChargeStorage", 1, diag, t)
{
    protected override object[] GetParameters(int step)
    {
        var total = 0f;
        foreach (var graph in Achievement._mechanicalGraphRegistry.MechanicalGraphs)
        {
            total += graph.CurrentPower.BatteryCharge;
        }

        return [
            total,
            BatteryChargeStorageAchievement.RequiredCharge
        ];
    }
}
