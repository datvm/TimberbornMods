namespace BeavlineLogistics.UI;

public class BeavlineFragment(
    ILoc t,
    EntityBadgeService entityBadgeService,
    EntitySelectionService entitySelectionService,
    IContainer container
) : BaseEntityPanelFragment<BeavlineComponent>
{
#nullable disable
    VisualElement pnlConnected;
    BeavlineNodePanel pnlInput, pnlOutput;
    Label lblOutputSpeed;
#nullable enable

    protected override void InitializePanel()
    {
        pnlInput = panel.AddChild(container.GetInstance<BeavlineNodePanel>).SetDisplay(false).SetMarginBottom();
        pnlInput.SetTitle(t.T("LV.BL.BeavlineIn"));
        pnlInput.OnDisabledChanged += v => component!.DisableInput = v;
        pnlInput.OnFilterChanged += v => component!.FilteredInput = v;

        pnlOutput = panel.AddChild(container.GetInstance<BeavlineNodePanel>).SetDisplay(false);
        pnlOutput.SetTitle(t.T("LV.BL.BeavlineOut"));
        pnlOutput.OnDisabledChanged += v => component!.SetOutputDisabled(v);
        pnlOutput.OnFilterChanged += v => component!.FilteredOutput = v;
        lblOutputSpeed = panel.AddGameLabel(name: "OutputSpeed").SetDisplay(false);

        panel.AddGameLabel(t.T("LV.BL.ConnectedTo")).SetMargin(top: 20);
        pnlConnected = panel.AddChild();
    }

    public override void ClearFragment()
    {
        base.ClearFragment();
        pnlConnected.Clear();
        pnlInput.ClearContent();
        pnlOutput.ClearContent();
        lblOutputSpeed.SetDisplay(false);
    }

    public override void ShowFragment(BaseComponent entity)
    {
        base.ShowFragment(entity);

        if (!component || !component.Active)
        {
            ClearFragment();
            return;
        }

        ShowData();
    }

    void ShowData()
    {
        if (component!.HasInput)
        {
            pnlInput.SetContent(component.DisableInput, component.InputGoodIds, component.FilteredInput);
        }

        if (component.HasOutput)
        {
            pnlOutput.SetContent(component.DisableOutput, component.OutputGoodIds, component.FilteredOutput);

            var outputSpeed = 1 / component.BeavlineOutput!.DaysPerItem;
            lblOutputSpeed.text = t.T("LV.BL.OutputSpeed", outputSpeed);
            lblOutputSpeed.SetDisplay(true);
        }

        foreach (var c in component.ConnectedBuildings)
        {
            var name = entityBadgeService.GetEntityName(c);
            var disabled = c.DisableInput || c.DisableOutput;

            pnlConnected.AddGameButton(name + (disabled ? (" " + t.T("LV.BL.Disabled")) : ""), onClick: () => SelectBuilding(c), stretched: true)
                .SetPadding(0, 5)
                .SetMarginBottom(3);
        }
    }

    void SelectBuilding(BeavlineComponent c) => entitySelectionService.SelectAndFocusOn(c);

}
