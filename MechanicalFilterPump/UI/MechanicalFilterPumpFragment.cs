global using Timberborn.Debugging;

namespace MechanicalFilterPump.UI;

public class MechanicalFilterPumpFragment(ILoc t, DevModeManager devs) : IEntityPanelFragment
{
    MechanicalFilterPumpComponent? comp;

#nullable disable // UI elements are not null
    EntityPanelFragmentElement panel;
    Toggle chkActive, chkNoPower;
    Label lblInfo;
#nullable enable

    public VisualElement InitializeFragment()
    {
        panel = new()
        {
            Background = EntityPanelFragmentBackground.Green,
        };

        chkActive = panel.AddToggle(t.T("LV.MFP.Filter"), onValueChanged: OnActiveChanged);
        lblInfo = panel.AddGameLabel();

        chkNoPower = panel.AddToggle(t.T("LV.MFP.FilterPowerCheat"), onValueChanged: OnNoPowerChanged)
            .SetMargin(left: 20);

        panel.ToggleDisplayStyle(false);
        return panel;
    }

    public void ClearFragment()
    {
        panel.Visible = false;
        comp = null;
    }

    public void ShowFragment(BaseComponent entity)
    {
        comp = entity.GetComponentFast<MechanicalFilterPumpComponent>();
        if (!comp) { return; }

        chkActive.value = comp.IsActive;
        chkNoPower.value = comp.NoPowerIncrease;
        lblInfo.text = t.T("LV.MFP.FilterPower", comp.PowerIncrease);

        chkNoPower.ToggleDisplayStyle(chkNoPower.value || devs.Enabled);

        panel.Visible = true;
    }

    public void UpdateFragment() { }

    void OnActiveChanged(bool active)
    {
        if (!comp) { return; }

        comp.SetActive(active);
    }

    void OnNoPowerChanged(bool noPower)
    {
        if (!comp) { return; }
        comp.SetPowerCheat(noPower);
    }

}
