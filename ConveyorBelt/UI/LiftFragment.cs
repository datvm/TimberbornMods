namespace ConveyorBelt.UI;

[BindFragment]
public class LiftFragment(ILoc t) : BaseEntityPanelFragment<BeltLift>
{
    Toggle sendingOut = null!;
    bool refreshing;

    protected override void InitializePanel() => sendingOut = panel.AddGamePanelToggle(t.T("LV.CBlt.LiftOut"), OnChanged);

    public override void ShowFragment(BaseComponent entity)
    {
        base.ShowFragment(entity);
        Refresh();
    }

    public override void UpdateFragment() => Refresh();

    void Refresh()
    {
        if (component is not BeltLift lift)
        {
            panel.Visible = false;
            return;
        }

        panel.Visible = true;
        refreshing = true;
        sendingOut.SetValueWithoutNotify(lift.SendingOut);
        refreshing = false;
    }

    void OnChanged(bool value)
    {
        if (refreshing || component is not BeltLift lift || lift.SendingOut == value)
        {
            return;
        }

        lift.Toggle();
    }
}
