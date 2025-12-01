namespace TImprove4Achievements.Services.Helpers;

public abstract class PlantTreesAchievementHelper<T>(
    TreePlantingCounter counter,
    DialogService diag,
    ILoc t
) : BaseMessageBoxAchievementHelper<T>("LV.T4A.PlantTree", 1, diag, t) where T : PlantTreesAchievement
{

    protected override object[] GetParameters(int step) => [
        counter._plantedCount.ToString("#,0"),
        Achievement._threshold.ToString("#,0")
    ];

}

public class Plant1000TreesAchievementHelper(TreePlantingCounter counter, DialogService diag, ILoc t) : PlantTreesAchievementHelper<Plant1000TreesAchievement>(counter, diag, t) { }
public class Plant5000TreesAchievementHelper(TreePlantingCounter counter, DialogService diag, ILoc t) : PlantTreesAchievementHelper<Plant5000TreesAchievement>(counter, diag, t) { }
public class Plant10000TreesAchievementHelper(TreePlantingCounter counter, DialogService diag, ILoc t) : PlantTreesAchievementHelper<Plant10000TreesAchievement>(counter, diag, t) { }