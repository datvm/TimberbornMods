namespace MoreAchievements.Achievements.Helpers;

[MultiBind(typeof(BaseAchievementHelper))]
public class OnlyEggplantsAchievementHelper(
    EntitySelectionService entitySelectionService,
    DialogService diag,
    ILoc t
) : BaseAchievementHelper<OnlyEggplantsAchievement>("LV.MA.OnlyEggplants", 1)
{
    public override void ActivateStep(int step)
    {
        var total = 0;
        foreach (var (illegal, amount) in Achievement.FindFoodStockpiles())
        {
            if (illegal is not null)
            {
                entitySelectionService.SelectAndFocusOn(illegal);
                diag.Alert("LV.MA.OnlyEggplants.Msg0Illegal", true);
                return;
            }

            total += amount;
        }

        if (total >= OnlyEggplantsAchievement.AmountRequired)
        {
            diag.Alert("LV.MA.OnlyEggplants.Msg0None", true);
        }
        else
        {
            diag.Alert(t.T("LV.MA.OnlyEggplants.Msg0NotEnough", total, OnlyEggplantsAchievement.AmountRequired));
        }
    }
}
