namespace MoreAchievements.Achievements.Helpers;

[MultiBind(typeof(BaseAchievementHelper))]
public class FillAllTanksBadwaterAchievementHelper(
    DialogService diag,
    ILoc t,
    EntitySelectionService entitySelectionService
)
    : BaseAchievementHelper<FillAllTanksBadwaterAchievement>("LV.MA.FillAllTanksBadwater", 1)
{

    public override void ActivateStep(int step)
    {
        var (illegal, amount) = Achievement.ValidateFulfillment();
        if (illegal is not null)
        {
            entitySelectionService.SelectAndFocusOn(illegal);
            diag.Alert("LV.MA.FillAllTanksBadwater.Msg0IllegalTank", true);
            return;
        }

        if (amount >= FillAllTanksBadwaterAchievement.MinimumBadwater)
        {
            diag.Alert("LV.MA.FillAllTanksBadwater.Msg0NoTank", true);
        }
        else
        {
            diag.Alert(t.T("LV.MA.FillAllTanksBadwater.Msg0NotEnough", amount, FillAllTanksBadwaterAchievement.MinimumBadwater));
        }
    }

}
