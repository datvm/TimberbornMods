namespace GateV1.UI;

public class GateEntityPanelFragment(ILoc t) : BaseEntityPanelFragment<GateComponent>
{
#nullable disable
    Toggle chkClose;
    Toggle chkAutoCloseHaz;
    Toggle chkAutoCloseBadtide;
#nullable enable

    protected override void InitializePanel()
    {
        chkClose = panel.AddToggle(t.T("LV.Gate.Closed"), onValueChanged: SetGateClosed);
        chkAutoCloseHaz = panel.AddToggle(t.T("LV.Gate.AutoCloseHaz"), onValueChanged: SetAutoCloseHaz);
        chkAutoCloseBadtide = panel.AddToggle(t.T("LV.Gate.AutoCloseBadtide"), onValueChanged: SetAutoCloseBadtide);
    }

    public override void ShowFragment(BaseComponent entity)
    {
        base.ShowFragment(entity);
        if (!component) { return; }

        chkClose.SetValueWithoutNotify(component!.Closed);
        chkAutoCloseHaz.SetValueWithoutNotify(component.AutoCloseHaz);
        chkAutoCloseBadtide.SetValueWithoutNotify(component.AutoCloseBadtide);
        SetAutoCloseUi();
        panel.Visible = component.IsFinished;
    }

    public override void UpdateFragment()
    {
        if (!panel.Visible)
        {
            panel.Visible = component && component!.IsFinished;
        }
    }


    void SetGateClosed(bool closed)
    {
        if (!component) { return; }

        component!.ToggleClosedState(closed);
    }

    void SetAutoCloseHaz(bool autoCloseHaz)
    {
        if (!component) { return; }

        component!.AutoCloseHaz = autoCloseHaz;
        SetAutoCloseUi();
    }

    void SetAutoCloseBadtide(bool autoCloseBadtide)
    {
        if (!component) { return; }

        component!.AutoCloseBadtide = autoCloseBadtide;
    }

    void SetAutoCloseUi()
    {
        chkAutoCloseBadtide.enabledSelf = !chkAutoCloseHaz.value;
    }


}
