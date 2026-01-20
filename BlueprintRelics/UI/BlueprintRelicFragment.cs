namespace BlueprintRelics.UI;

[BindFragment]
public class BlueprintRelicFragment(
    ILoc t,
    BlueprintRelicCollectorService collectorService,
    IGoodService goodService,
    BlueprintRelicRewardUIController rewardUIController,
    NamedIconProvider namedIconProvider,
    DialogService diag
) : BaseEntityPanelFragment<BlueprintRelicCollector>
{

#nullable disable
    ProgressBar pgbExpiry; Label lblExpiry;
    Toggle chkPause;
    Label lblConnection;
    ProgressBar pgbSteps; Label lblSteps;
    ProgressBar pgbCurrent; Label lblCurrent;
    Button btnReward;
    VisualElement lstStepRequirements;
    Button btnNegotiate;
#nullable enable

    bool isShowingNegotiationCooldown;

    protected override void InitializePanel()
    {
        pgbExpiry = panel.AddProgressBar().SetColor(ProgressBarColor.Blue).SetMarginBottom();
        lblExpiry = pgbExpiry.AddProgressLabel();

        lblConnection = panel.AddGameLabel();
        chkPause = panel.AddToggle(t.T("LV.BRe.PauseExcavation"), onValueChanged: TogglePause)
            .SetWrap().SetMarginBottom().SetMaxSizePercent(100, null);

        var stepPanel = panel.AddChild().SetMarginBottom();

        pgbSteps = stepPanel.AddProgressBar().SetColor(ProgressBarColor.Green).SetMarginBottom(10);
        lblSteps = pgbSteps.AddProgressLabel();

        pgbCurrent = stepPanel.AddProgressBar().SetColor(ProgressBarColor.Green).SetMarginBottom(10);
        lblCurrent = pgbCurrent.AddProgressLabel();

        stepPanel.AddGameLabel(t.T("LV.BRe.ExcInfo")).SetMarginBottom(10);
        lstStepRequirements = stepPanel.AddChild().SetMarginBottom(10);
        btnNegotiate = stepPanel.AddGameButtonPadded(t.T("LV.BRe.Negotiate"), onClick: RequestNegotiation, stretched: true);

        btnReward = panel.AddStretchedEntityFragmentButton(t.T("LV.BRe.ChooseReward"), onClick: PickReward, color: EntityFragmentButtonColor.Red);
        btnReward.style.unityTextAlign = TextAnchor.MiddleCenter;
    }

    public override void ShowFragment(BaseComponent entity)
    {
        base.ShowFragment(entity);
        if (component is null) { return; }

        UpdateRequirements();
        chkPause.SetValueWithoutNotify(component.PauseCollecting);
    }

    void UpdateRequirements()
    {
        if (!component) { return; }

        lstStepRequirements.Clear();
        var row = lstStepRequirements.AddRow().AlignItems().JustifyContent(Justify.SpaceEvenly);
        row.AddIconSpan()
            .SetVertical()
            .SetContent(namedIconProvider.Clock, prefixText: t.T("Time.DaysShort", component!.StepDays.ToString("0.0")), size: 24);
        row.AddIconSpan().SetScience(namedIconProvider, component.ScienceRequirement.ToString());

        foreach (var g in component.RequiredGoods)
        {
            row.AddIconSpan().SetGood(goodService, g.GoodId, g.Amount.ToString()).SetVertical();
        }
    }

    async void RequestNegotiation()
    {
        if (!component ||
            !component!.CanNegotiate ||
            !await diag.ConfirmAsync(t.T("LV.BRe.NegotiateConfirm", component.NegotiateCooldownDays)))
        {
            return;
        }

        component.Negotiate();
        UpdateRequirements();
    }

    public override void UpdateFragment()
    {
        if (!component) { return; }

        btnReward.enabledSelf = component!.Finished;
        btnNegotiate.enabledSelf = component.CanNegotiate;
        if (component.NegotiateCooldownTicks > 0)
        {
            btnNegotiate.text = t.T("LV.BRe.NegotiateCooldown", (GetDaysFromTicks(component.NegotiateCooldownTicks) * 24f).ToString("0.0"));
            isShowingNegotiationCooldown = true;
        }
        else if (isShowingNegotiationCooldown)
        {
            btnNegotiate.text = t.T("LV.BRe.Negotiate");
            isShowingNegotiationCooldown = false;
        }

        var remainingDays = GetDaysFromTicks(component.ExpiryTicks);
        pgbExpiry.SetProgress(remainingDays / component.TotalDays, lblExpiry, t.T("LV.BRe.ExpiresIn", remainingDays.ToString("0.0")));

        var district = component.ConnectedDistrict;
        lblConnection.text = district
            ? t.T("LV.BRe.Connected", district!.DistrictName)
            : t.T("LV.BRe.ConnectPrompt");

        var completedSteps = component.TotalSteps - component.StepsLeft;
        pgbSteps.SetProgress((float)completedSteps / component.TotalSteps,
            lblSteps, t.T("LV.BRe.ExcSteps", completedSteps, component.TotalSteps));

        if (component.IsExcavating)
        {
            var remainingStepDays = GetDaysFromTicks(component.StepTickLeft);
            var currStepCompletedInDays = component.StepDays - remainingStepDays;
            var currStepProgress = currStepCompletedInDays / component.StepDays;

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
        component = null;
        lstStepRequirements.Clear();
    }

    void PickReward()
    {
        rewardUIController.PickReward(component!);
    }

    void TogglePause(bool v) => component!.PauseCollecting = v;

    float GetDaysFromTicks(int ticks) => ticks / collectorService.TicksInDay;

}
