namespace BlueprintRelics.UI.RewardPicker;

[BindTransient]
public class ScienceRewardPanel(ILoc t) : RewardPanelBase<ScienceReward>(t)
{
    
    protected override void InitializeUI()
    {
        this.AddGameLabel(t.T("LV.BRe.ScienceRewardDesc", Reward.Amount));
    }

}
