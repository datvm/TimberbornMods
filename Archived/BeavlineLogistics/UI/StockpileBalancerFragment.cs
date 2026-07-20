
namespace BeavlineLogistics.UI;

public class StockpileBalancerFragment(
    ILoc t
) : BaseEntityPanelFragment<BeavlineBalancerComponent>
{

#nullable disable
    Toggle chkDisable, chkDisableEntranceWarning;
#nullable enable

    protected override void InitializePanel()
    {
        chkDisableEntranceWarning = panel.AddToggle(t.T("LV.BL.RemoveEntranceWarning"), onValueChanged: OnDisableEntranceWarningChanged)
            .SetMarginBottom(10)
            .SetDisplay(false);
        chkDisable = panel.AddToggle(t.T("LV.BL.DisableBalancer"), onValueChanged: OnDisabledChanged)
            .SetMarginBottom(10)
            .SetDisplay(false);
    }

    public override void ClearFragment()
    {
        base.ClearFragment();
        chkDisableEntranceWarning.SetDisplay(false);
        chkDisable.SetDisplay(false);
    }

    public override void ShowFragment(BaseComponent entity)
    {
        base.ShowFragment(entity);
        if (!component) { return; }

        var hasBalancer = component.RenovationActive;
        chkDisable.SetDisplay(hasBalancer);
        chkDisable.SetValueWithoutNotify(component.Disabled);

        var canDisableWarning = hasBalancer || component.GetComponentFast<BeavlineComponent>().Active;
        chkDisableEntranceWarning.SetDisplay(canDisableWarning);
        chkDisableEntranceWarning.SetValueWithoutNotify(component.DisableEntranceWarning);
    }

    void OnDisableEntranceWarningChanged(bool v)
    {
        if (!component) { return; }
        component.DisableEntranceWarning = v;
    }

    void OnDisabledChanged(bool v)
    {
        if (!component) { return; }
        component.Disabled = v;
    }

}
