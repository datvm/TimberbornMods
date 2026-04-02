namespace MoreAchievements.Achievements.Helpers;

[MultiBind(typeof(BaseAchievementHelper))]
public class StackAllFoodWarehousesAchievementHelper(DialogService diag, ILoc t)
    : BaseMessageBoxAchievementHelper<StackAllFoodWarehousesAchievement>("LV.MA.StackAllFoodWarehouses", 1, diag, t)
{
    protected override object[] GetParameters(int step) => [string.Join(", ", Achievement.RequiredGoodNames)];
}
