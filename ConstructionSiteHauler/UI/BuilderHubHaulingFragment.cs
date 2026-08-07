namespace ConstructionSiteHauler.UI;

[BindFragment]
public class BuilderHubHaulingFragment(ILoc t) : BaseEntityPanelFragment<BuilderHubHaulingDisabler>
{
    Toggle chkDisableHauling = null!;

    protected override void InitializePanel()
    {
        chkDisableHauling = panel.AddGamePanelToggle(
            t.T("LV.CSH.DisableBuilderHaul"),
            onValueChanged: OnDisabledChanged);
    }

    public override void ShowFragment(BaseComponent entity)
    {
        base.ShowFragment(entity);
        if (!component) { return; }

        chkDisableHauling.SetValueWithoutNotify(component!.DisableHaulingMaterials);
    }

    void OnDisabledChanged(bool disabled)
    {
        if (!component) { return; }

        component!.DisableHaulingMaterials = disabled;
    }
}
