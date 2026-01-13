namespace BlueprintRelics.UI;

public class RelicRewardDialog(PanelStack panelStack) : DialogBoxElement
{
    public IRelicReward? SelectedReward { get; private set; }

    public void SetRewards(IReadOnlyList<IRelicReward> rewards)
    {

    }

    public async Task<IRelicReward?> ShowDialogAsync()
    {
        if (!await ShowAsync(null, panelStack))
        {
            SelectedReward = null;
        }

        return SelectedReward;
    }

}
