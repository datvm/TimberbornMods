namespace StockpileBalancer.UI;

[BindFragment]
public class StockpileBalancerFragment(
    ILoc t,
    EntitySelectionService entitySelectionService,
    EntityBadgeService entityBadgeService
) : BaseEntityPanelFragment<StockpileBalancerComponent>
{
    static readonly BalancerGroup MarkerGroup = new("", []);

#nullable disable
    Toggle chkDisabled;
    VisualElement balancersPanel;
    Label lblBalanced;
#nullable enable
    BalancerGroup? currGrp = MarkerGroup;

    protected override void InitializePanel()
    {
        chkDisabled = panel.AddGamePanelToggle(t.T("LV.SBl.DisableBalancer"), OnDisabledChanged).SetMarginBottom();

        var labelRow = panel.AddRow().AlignItems().SetMarginBottom();
        labelRow.AddLabel(t.T("LV.SBl.BalancerGroup")).SetMarginBottom(5);
        lblBalanced = labelRow.AddLabel(t.T("LV.SBl.Balanced")).SetMarginLeftAuto().SetDisplay(false);

        balancersPanel = panel.AddChild();
    }

    public override void UpdateFragment()
    {
        if (!component || !component!.HasBalancer)
        {
            panel.Visible = false;
            return;
        }

        panel.Visible = true;
        chkDisabled.SetValueWithoutNotify(component.BalancerDisabled);

        var grp = component.BalancerGroup;
        if (grp != currGrp)
        {
            currGrp = grp;
            PopulateConnectors();
        }

        lblBalanced.SetDisplay(grp?.Balanced == true);
    }

    void PopulateConnectors()
    {
        balancersPanel.Clear();

        if (component!.BalancerGroup is not { } grp || grp.Balancers.Count <= 1)
        {
            balancersPanel.AddLabel(t.T("LV.SBl.BalancerGroupNone"));
            return;
        }

        foreach (var b in grp.Balancers)
        {
            var avatar = entityBadgeService.GetEntityAvatar(b);
            var name = b.GetName(t);

            var row = balancersPanel.AddRow().AlignItems().SetMarginBottom(5);
            row.AddImage(avatar).SetSize(24).SetMarginRight(5).SetFlexShrink(0);
            row.AddLabel(name).SetFlexGrow().SetMarginRight();

            if (b != component)
            {
                row.AddGameButtonPadded(t.T("LV.SBl.Select"), () => Select(b));
            }
        }
    }

    public override void ClearFragment()
    {
        base.ClearFragment();
        balancersPanel.Clear();
        currGrp = MarkerGroup;
    }

    void OnDisabledChanged(bool disabled)
    {
        if (!component) { return; }

        component!.SetBalancerDisabled(disabled);
    }

    void Select(StockpileBalancerComponent comp) => entitySelectionService.SelectAndFocusOn(comp);

}
