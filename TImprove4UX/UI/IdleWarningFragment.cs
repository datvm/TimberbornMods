namespace TImprove4UX.UI;

[Description(CollapsibleEntityPanelService.DoNotCollapseTag)]
public class IdleWarningFragment(
    ILoc t,
    MSettings s
) : BaseEntityPanelFragment<WorkerIdleWarningComponent>
{

#nullable disable
    Toggle chkDisableWarning;
#nullable enable

    protected override void InitializePanel()
    {
        chkDisableWarning = panel.AddToggle(name: "DisableIdleWarning", onValueChanged: v => component!.ToggleDisableWarning(v));
    }

    public override void ShowFragment(BaseComponent entity)
    {
        if (!s.WorkerIdleWarning.Value) { return; } 

        base.ShowFragment(entity);
        if (!component) { return; }

        var label = component!.GetComponent<LabeledEntity>();
        var name = label.DisplayName;

        chkDisableWarning.text = t.T("LV.T4UX.WorkerIdleWarningDisable", name);
        chkDisableWarning.SetValueWithoutNotify(component.DisableWarning);
    }

}
