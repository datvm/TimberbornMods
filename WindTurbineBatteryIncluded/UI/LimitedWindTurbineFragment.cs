namespace WindTurbineBatteryIncluded.UI;

[BindFragment]
public class LimitedWindTurbineFragment(
    ILoc t,
    WindService winds,
    LimitedWindTurbineService turbines
) : BaseEntityPanelFragment<LimitedWindTurbineComponent>
{
    Label lblWindStrength = null!;
    Label lblDirection = null!;

    protected override void InitializePanel()
    {
        lblWindStrength = panel.AddLabel().SetMarginBottom(5);
        lblDirection = panel.AddLabel().SetMarginBottom(5);
    }

    public override void UpdateFragment()
    {
        if (component is not { } turbine || !turbine) { return; }

        var power = turbines.Horsepower(turbine.NominalPower, turbine.YawError, turbine.Spec);

        lblWindStrength.text = t.T("LV.WTB.GeneratorWindStr", winds.WindStrength, power.Strength);
        lblDirection.text = "LV.WTB.GeneratorWindStrDirection".TFormat(t, turbine.YawError, turbine.Spec.MaxDegree, power.Direction, power.Alignment);
    }

}
