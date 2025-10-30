namespace StableCore.UI;

public class StableCoreFragment(
    ILoc t
) : BaseEntityPanelFragment<StablizedCoreComponent>
{

#nullable disable
    Label lblArmDesc;
#nullable enable

    protected override void InitializePanel()
    {
        panel.AddGameButtonPadded(t.T("LV.StC.Arm"), onClick: Arm, stretched: true).SetMarginBottom(5);
        lblArmDesc = panel.AddGameLabel(t.T("LV.StC.ArmDesc", 10));
    }

    public override void ShowFragment(BaseComponent entity)
    {
        base.ShowFragment(entity);
        if (component is null) { return; }

        if (component.Armed)
        {
            ClearFragment();
            return;
        }

        lblArmDesc.text = t.T("LV.StC.ArmDesc", component.ArmingDays);
    }

    void Arm()
    {
        component?.Arm();
        ClearFragment();
    }

}
