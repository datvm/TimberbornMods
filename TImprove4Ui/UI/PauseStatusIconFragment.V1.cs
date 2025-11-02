namespace TImprove4Ui.UI;

[System.ComponentModel.Description("DoNotCollapse")]
public class PauseStatusIconFragment(
    PauseStatusIconRegistry registry,
    MSettings s,
    ILoc t
) : BaseEntityPanelFragment<StatusTracker>
{

#nullable disable
    Toggle chkDisable;
#nullable enable
    string? prefabName;

    protected override void InitializePanel()
    {
        chkDisable = panel.AddToggle(name: "DisablePauseIcon", onValueChanged: OnDisabledChanged);
        chkDisable.style.whiteSpace = WhiteSpace.Normal;
    }

    public override void ShowFragment(BaseComponent entity)
    {
        base.ShowFragment(entity);
        if (!s.ShowDisablePauseIcon.Value
            || !component
            || !registry.HasPauseStatus(component!.PrefabName))
        {
            ClearFragment();
            return;
        }

        var spec = component.GetComponent<TemplateSpec>();
        prefabName = spec.TemplateName;

        var label = component.GetComponent<LabeledEntity>();
        chkDisable.text = t.T("LV.T4UI.DisablePauseIcon", label.DisplayName);

        chkDisable.SetValueWithoutNotify(registry.ShouldDisable(prefabName));
    }

    void OnDisabledChanged(bool disabled)
    {
        if (prefabName is null) { return; }

        if (disabled)
        {
            registry.AddDisabledPrefab(prefabName);
        }
        else
        {
            registry.RemoveDisabledPrefab(prefabName);
        }
    }

}
