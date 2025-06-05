namespace Gate.UI;

public class GateEntityPanelFragment(
    ILoc t
) : IEntityPanelFragment
{

#nullable disable
    EntityPanelFragmentElement panel;
    Toggle chkClose;
#nullable enable

    GateComponent? comp;

    public void ClearFragment()
    {
        panel.Visible = false;
        comp = null;
    }

    public VisualElement InitializeFragment()
    {
        panel = new() { Visible = false, };

        chkClose = panel.AddToggle(t.T("LV.Gate.Closed"), onValueChanged: SetGateClosed);

        return panel;
    }

    public void ShowFragment(BaseComponent entity)
    {
        comp = entity.GetComponentFast<GateComponent>();
        if (!comp) { return; }

        chkClose.SetValueWithoutNotify(comp.Closed);
        panel.Visible = true;
    }

    public void UpdateFragment() { }

    void SetGateClosed(bool closed)
    {
        if (!comp) { return; }

        comp.ToggleClosedState(closed);
    }

}
