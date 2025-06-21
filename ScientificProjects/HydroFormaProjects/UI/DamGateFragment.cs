namespace HydroFormaProjects.UI;

public class DamGateFragment(
    ILoc t,
    DamGateService damGates
) : IEntityPanelFragment
{

#nullable disable
    EntityPanelFragmentElement panel;

    Toggle chkClosed, chkSync;
#nullable enable

    DamGateComponent? comp;

    public void ClearFragment()
    {
        panel.Visible = false;
        comp = null;
    }

    public VisualElement InitializeFragment()
    {
        panel = new()
        {
            Visible = false,
        };

        chkSync = panel.AddToggle(t.T("LV.HF.DamSync"), onValueChanged: v => comp!.Synchronize = v)
            .SetMarginBottom();
        chkClosed = panel.AddToggle(t.T("LV.HF.Closed"), onValueChanged: ToggleDamGate);

        return panel;
    }

    public void ShowFragment(BaseComponent entity)
    {
        var comp = entity.GetComponentFast<DamGateComponent>();
        if (!comp) { return; }

        chkSync.SetValueWithoutNotify(comp.Synchronize);
        UpdateFragment();
        panel.Visible = true;
    }

    public void UpdateFragment()
    {
        if (!comp) { return; }

        chkClosed.SetValueWithoutNotify(comp.Closed);
    }

    void ToggleDamGate(bool closed)
    {
        damGates.ToggleDamGate(comp!, closed);
    }

}
