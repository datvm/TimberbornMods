
namespace TImprove4Achievements.Services.Helpers;

public abstract class GeneratePowerWithAchievementHelper<T, TComp>(DialogService diag, ILoc t)
    : BaseMessageBoxAchievementHelper<T>("LV.T4A.GeneratePower", 1, diag, t)
    where T : GeneratePowerWithAchievement<TComp>
{

    protected override object[] GetParameters(int step)
    {
        var max = 0;
        foreach (MechanicalGraph graphCandidate in Achievement._graphCandidates)
        {
            if (graphCandidate is null) { continue; }

            var power = graphCandidate.CurrentPower.PowerSupply;
            if (power > max)
            {
                max = power;
            }
        }

        return [typeof(TComp).Name, max, Achievement._requiredPower];
    }

}

public class GeneratePowerWithWaterWheelsOnlyAchievementHelper(DialogService diag, ILoc t)
    : GeneratePowerWithAchievementHelper<GeneratePowerWithWaterWheelsOnlyAchievement, WaterPoweredGenerator>(diag, t)
{ }

public class GeneratePowerWithPowerWheelsOnlyAchievementHelper(DialogService diag, ILoc t)
    : GeneratePowerWithAchievementHelper<GeneratePowerWithPowerWheelsOnlyAchievement, WalkerPoweredGenerator>(diag, t)
{ }

public class GeneratePowerWithWindTurbinesOnlyAchievementHelper(DialogService diag, ILoc t)
    : GeneratePowerWithAchievementHelper<GeneratePowerWithWindTurbinesOnlyAchievement, WindPoweredGenerator>(diag, t)
{ }