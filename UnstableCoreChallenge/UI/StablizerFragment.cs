namespace UnstableCoreChallenge.UI;

[BindFragment]
public class StablizerFragment(
    ILoc t,
    ScienceService scienceService,
    NamedIconProvider namedIconProvider,
    IGoodService goodService
) : BaseEntityPanelFragment<UnstableCoreStabilizer>
{
    VisualElement goodList = null!;
    readonly Dictionary<string, GoodRow> goods = [];

    VisualElement sciencePanel = null!;
    Button btnPayScience = null!;
    Label lblScience = null!;
    float reqScience;

    protected override void InitializePanel()
    {
        panel.AddLabel(t.T("LV.USC.RequiredPayments")).SetMarginBottom(10);
        sciencePanel = panel.AddRow().AlignItems().SetMarginBottom(10).SetDisplay(false);
        goodList = panel.AddChild();

        sciencePanel.AddIconSpan(namedIconProvider.Science, postfixText: t.T("LV.USC.Science"), size: 24);
        sciencePanel.AddChild().SetMarginLeftAuto();
        btnPayScience = sciencePanel.AddGameButtonPadded(t.T("LV.USC.Pay"), onClick: PayScience).SetMarginRight(5);
        lblScience = sciencePanel.AddLabel();
        btnPayScience.enabledSelf = false;
    }

    public override void ShowFragment(BaseComponent entity)
    {
        base.ShowFragment(entity);
        if (!component) { return; }

        foreach (var p in component!.GetCurrentPayments())
        {
            if (p.IsScience)
            {
                reqScience = p.Amount;
                lblScience.text = GetScienceText();
                btnPayScience.enabledSelf = false;
                btnPayScience.SetDisplay(false);
                sciencePanel.SetDisplay(true);
                continue;
            }

            var goodRow = goodList.AddRow();
            var goodIcon = goodRow.AddIconSpan().SetGood(goodService, p.GoodId, showName: true);
            var goodLabel = goodRow.AddLabel().SetMarginLeftAuto().SetMarginBottom(5);

            goods.Add(p.GoodId, new(p.GoodId, goodIcon, goodLabel));
        }
    }

    public override void UpdateFragment()
    {
        if (!component) { return; }

        foreach (var g in component!.GetCurrentPayments())
        {
            if (g.IsScience)
            {
                lblScience.text = GetScienceText();
                if (g.Finished)
                {
                    btnPayScience.SetDisplay(false);
                }
                else
                {
                    btnPayScience.SetDisplay(true);
                    btnPayScience.enabledSelf = scienceService.SciencePoints >= reqScience;
                }
            }
            else
            {
                var row = goods[g.GoodId];
                var finished = g.Finished;

                if (finished)
                {
                    row.QualityLabel.style.color = row.GoodLabel.PostfixLabel!.style.color = TimberUiUtils.SuccessColor;
                }

                row.QualityLabel.text = $"{g.Paid} / {g.Amount}";
            }
        }
    }

    public override void ClearFragment()
    {
        base.ClearFragment();

        reqScience = 0;
        sciencePanel.SetDisplay(false);
        goods.Clear();
        goodList.Clear();
    }

    string GetScienceText()
    {
        string currStr;

        if (component!.SciencePaid)
        {
            currStr = t.T("LV.USC.Paid").Color(TimberbornTextColor.Green);
        }
        else
        {
            var curr = scienceService.SciencePoints;
            currStr = curr.ToString().Color(curr >= reqScience ? TimberbornTextColor.Green : TimberbornTextColor.Red);
        }

        return $"{currStr} / {reqScience}";
    }

    void PayScience()
    {
        if (!component || component!.SciencePayment == 0) { return; }

        var science = component.GetCurrentPayments().First(p => p.IsScience);
        if (science.Finished) { return; }

        var remainingAmount = science.Amount - science.Paid; // Science paid should be 0, but just in case.
        if (scienceService.SciencePoints < remainingAmount) { return; }

        scienceService.SubtractPoints(remainingAmount);
        component.Pay(UnstableCoreSpecService.Science, remainingAmount);
    }

    readonly record struct GoodRow(string GoodId, IconSpan GoodLabel, Label QualityLabel);
}
