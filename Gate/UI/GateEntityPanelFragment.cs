namespace Gate.UI;

public class GateEntityPanelFragment(
    ILoc t
) : IEntityPanelFragment
{

#nullable disable
    EntityPanelFragmentElement panel;
    Toggle chkClose;
    Toggle chkAutoCloseHaz;
    Toggle chkAutoCloseBadtide;
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
        chkAutoCloseHaz = panel.AddToggle(t.T("LV.Gate.AutoCloseHaz"), onValueChanged: SetAutoCloseHaz);
        chkAutoCloseBadtide = panel.AddToggle(t.T("LV.Gate.AutoCloseBadtide"), onValueChanged: SetAutoCloseBadtide);

        return panel;
    }

    public void ShowFragment(BaseComponent entity)
    {
        comp = entity.GetComponentFast<GateComponent>();
        if (!comp) { return; }

        chkClose.SetValueWithoutNotify(comp.Closed);
        chkAutoCloseHaz.SetValueWithoutNotify(comp.AutoCloseHaz);
        chkAutoCloseBadtide.SetValueWithoutNotify(comp.AutoCloseBadtide);
        SetAutoCloseUi();
        panel.Visible = comp.IsFinished;
    }

    public void UpdateFragment()
    {
        if (!panel.Visible)
        {
            panel.Visible = comp && comp.IsFinished;
        }
    }

    void SetGateClosed(bool closed)
    {
        if (!comp) { return; }

        comp.ToggleClosedState(closed);
    }

    void SetAutoCloseHaz(bool autoCloseHaz)
    {
        if (!comp) { return; }
        
        comp.AutoCloseHaz = autoCloseHaz;
        SetAutoCloseUi();
    }

    void SetAutoCloseBadtide(bool autoCloseBadtide)
    {
        if (!comp) { return; }
        
        comp.AutoCloseBadtide = autoCloseBadtide;
    }

    void SetAutoCloseUi()
    {
        chkAutoCloseBadtide.enabledSelf = !chkAutoCloseHaz.value;
    }

}
