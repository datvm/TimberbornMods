namespace BuildingHP.UI;

public class RenovationStockpileFragment(
    ILoc t,
    RenovationPriorityToggleGroupFactory priorityToggleGroupFactory
) : BaseEntityPanelFragment<BuildingRenovationStockpileComponent>
{

#nullable disable
    Toggle chkSupply;
    PriorityToggleGroup grpPriority;
#nullable enable

    protected override void InitializePanel()
    {
        var priorityPanel = panel.AddChild().SetMarginBottom(5);
        grpPriority = priorityToggleGroupFactory.CreateForStockpile(priorityPanel);

        chkSupply = panel.AddToggle(t.T("LV.BHP.SupplyRenovation"), onValueChanged: OnSupplyChanged);
    }

    public override void ShowFragment(BaseComponent entity)
    {
        base.ShowFragment(entity);
        if (!component) { return; }

        grpPriority.Enable(component);
        grpPriority.UpdateGroup();

        chkSupply.SetValueWithoutNotify(component.Supply);
    }

    public override void UpdateFragment()
    {
        if (!component) { return; }
        grpPriority.UpdateGroup();
    }

    void OnSupplyChanged(bool enabled)
    {
        if (!component) { return; }
        component.SetSupply(enabled);
    }

}
