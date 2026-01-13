namespace BlueprintRelics.UI;

public class BlueprintRelicFragment(
    ILoc t,
    BlueprintRelicCollectorService collectorService,
    IGoodService goodService,
    BlueprintRewardUIController rewardUIController
) : BaseEntityPanelFragment<BlueprintRelicComponent>
{

    BlueprintRelicCollector? collector;

#nullable disable
    ProgressBar pgbExpiry; Label lblExpiry;
    Toggle chkPause;
    Label lblConnection;
    ProgressBar pgbSteps; Label lblSteps;
    ProgressBar pgbCurrent; Label lblCurrent;
    Label lblStepInfo;
    Button btnReward;
#nullable enable

    protected override void InitializePanel()
    {
        pgbExpiry = panel.AddProgressBar().SetColor(ProgressBarColor.Blue).SetMarginBottom();
        lblExpiry = pgbExpiry.AddProgressLabel();

        chkPause = panel.AddToggle(t.T("LV.BRe.PauseExcavation"), onValueChanged: TogglePause).SetMarginBottom();

        lblConnection = panel.AddGameLabel().SetMarginBottom();

        pgbSteps = panel.AddProgressBar().SetColor(ProgressBarColor.Green).SetMarginBottom(10);
        lblSteps = pgbSteps.AddProgressLabel();

        pgbCurrent = panel.AddProgressBar().SetColor(ProgressBarColor.Green).SetMarginBottom(10);
        lblCurrent = pgbCurrent.AddProgressLabel();

        lblStepInfo = panel.AddGameLabel().SetMarginBottom();

        btnReward = panel.AddEntityFragmentButton(t.T("LV.BRe.ChooseReward"), onClick: PickReward);
    }

    public override void ShowFragment(BaseComponent entity)
    {
        base.ShowFragment(entity);
        if (component is null) { return; }

        collector = component.GetComponent<BlueprintRelicCollector>();

        chkPause.SetValueWithoutNotify(collector.PauseCollecting);
        lblStepInfo.text = DescribeStepInfo(collector);
    }

    string DescribeStepInfo(BlueprintRelicCollector collector)
    {
        return t.T("LV.BRe.ExcInfo",
            DescribeGoodList(),
            collector.ScienceRequirement,
            collector.StepDays.ToString("0.0"));

        string DescribeGoodList()
            => string.Join(Environment.NewLine, collector.RequiredGoods
                .Select(g => t.T("LV.BRe.ExcGood", goodService.GetGood(g.GoodId).DisplayName.Value, g.Amount)));
    }

    public override void UpdateFragment()
    {
        if (!collector) { return; }

        btnReward.enabledSelf = collector!.Finished;

        var remainingDays = GetDaysFromTicks(collector.ExpiryTicks);
        pgbExpiry.SetProgress(remainingDays / collector.TotalDays, lblExpiry, t.T("LV.BRe.ExpiresIn", remainingDays.ToString("0.0")));

        var district = component?.ConnectedDistrict;
        lblConnection.text = district
            ? t.T("LV.BRe.Connected", district!.DistrictName)
            : t.T("LV.BRe.ConnectPrompt");

        var completedSteps = collector.TotalSteps - collector.StepsLeft;
        pgbSteps.SetProgress((float)completedSteps / collector.TotalSteps,
            lblSteps, t.T("LV.BRe.ExcSteps", completedSteps, collector.TotalSteps));

        if (collector.IsExcavating)
        {
            var remainingStepDays = GetDaysFromTicks(collector.StepTickLeft);
            var currStepCompletedInDays = collector.StepDays - remainingStepDays;
            var currStepProgress = currStepCompletedInDays / collector.StepDays;

            pgbCurrent.SetProgress(currStepProgress,
                lblCurrent, t.T("LV.BRe.CurrStep", remainingStepDays.ToString("0.0")));
            pgbCurrent.SetDisplay(true);
        }
        else
        {
            pgbCurrent.SetDisplay(false);
        }

    }

    public override void ClearFragment()
    {
        base.ClearFragment();
        collector = null;
    }

    void PickReward()
    {
        rewardUIController.PickReward(collector!);
    }

    void TogglePause(bool v) => collector!.PauseCollecting = v;

    float GetDaysFromTicks(int ticks) => ticks / collectorService.TicksInDay;

}
