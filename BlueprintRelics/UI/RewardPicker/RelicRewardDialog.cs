namespace BlueprintRelics.UI.RewardPicker;

[BindTransient]
public class RelicRewardDialog(
    PanelStack panelStack,
    ILoc t,
    DialogService diags,
    IContainer container,
    VisualElementInitializer veInit
) : DialogBoxElement
{
    public IRelicReward? SelectedReward => selectedPanel?.Reward;
    IRewardPanel? selectedPanel;

    public event Action OnUnlockedDialogRequested = null!;

#nullable disable
    VisualElement lstRewards;
    Button btnConfirm;
#nullable enable

    public void SetRewards(IReadOnlyList<IRelicReward> rewards)
    {
        SetTitle(t.T("LV.BRe.ChooseReward"));
        AddCloseButton(DigMore);
        SetDialogPercentSize(height: .8f);

        var parent = Content;

        var statRow = parent.AddRow().AlignItems();
        statRow.AddChild(container.GetInstance<RecipeStatPanel>).SetFlexGrow();
        statRow.AddGameButtonPadded(t.T("LV.BRe.OpenUpgradeDialog"), onClick: () => OnUnlockedDialogRequested())
            .SetFlexShrink(0);

        lstRewards = parent.AddChild().SetMarginBottom();
        foreach (var r in rewards)
        {
            var panelType = r switch
            {
                RecipeUnlockUpgrade => typeof(RecipeUnlockRewardPanel),
                ScienceReward => typeof(ScienceRewardPanel),
                RecipeCapacityUpgradeReward => typeof(RecipeCapacityUpgradeRewardPanel),
                RecipeTimeReductionUpgradeReward => typeof(RecipeTimeReductionUpgradeRewardPanel),
                RecipeOutputUpgradeReward => typeof(RecipeOutputUpgradeRewardPanel),
                _ => throw new NotImplementedException($"Reward type {r.GetType().Name} is not known"),
            };

            var panel = (IRewardPanel)container.GetInstance(panelType);
            panel.SetReward(r);
            panel.OnSelected += OnRewardSelected;

            lstRewards.Add((VisualElement) panel);
        }

        btnConfirm = parent.AddMenuButton(t.T("LV.BRe.ConfirmReward"), onClick: OnUIConfirmed, size: UiBuilder.GameButtonSize.Large, stretched: true)
            .SetMarginBottom();
        btnConfirm.enabledSelf = false;

        var digPanel = parent.AddChild();
        digPanel.AddGameButtonPadded(t.T("LV.BRe.DigMore"), onClick: DigMore, stretched: true);
        digPanel.AddGameLabel(t.T("LV.BRe.DigMoreDesc"));
    }

    async void DigMore()
    {
        var confirm = await diags.ConfirmAsync("LV.BRe.DigConfirm", true);
        if (confirm)
        {
            OnUICancelled();
        }
    }

    void OnRewardSelected(object sender, EventArgs e)
    {
        selectedPanel = sender as IRewardPanel;

        foreach (var c in lstRewards.Children())
        {
            if (c != selectedPanel && c is IRewardPanel p)
            {
                p.Unselect();
            }
        }

        btnConfirm.enabledSelf = selectedPanel is not null;
    }

    public async Task<IRelicReward?> ShowDialogAsync()
    {
        if (!await ShowAsync(veInit, panelStack))
        {
            selectedPanel = null;
        }

        return SelectedReward;
    }

}
