namespace WarningsBeGone.UI;

public class StatusHidingFragment(
    ILoc t,
    StatusHidingService service
) : BaseEntityPanelFragment<StatusHidingComponent>
{

#nullable disable
    VisualElement list;
    Toggle chkHideCorner;
#nullable enable

    protected override void InitializePanel()
    {
        chkHideCorner = panel.AddToggle(t.T("LV.WBG.HideCorner"), onValueChanged: OnHideCornerChanged)
            .SetMarginBottom();

        list = panel.AddChild();
    }

    public override void ShowFragment(BaseComponent entity)
    {
        base.ShowFragment(entity);
        if (component is null) { return; }

        chkHideCorner.SetValueWithoutNotify(StatusHidingService.HideCornerWarnings);
        ShowStatuses();
    }

    public override void ClearFragment()
    {
        base.ClearFragment();

        list.Clear();
    }

    void ShowStatuses()
    {        
        var statuses = component!.GatherStatuses().ToArray();
        if (statuses.Length == 0)
        {
            ClearFragment();
            return;
        }

        var name = component.DisplayName;
        var templateList = service.GetHidingStatusesForTemplate(component.TemplateName);

        foreach (var status in statuses)
        {
            var panel = list.AddChild(() => new SingleStatusPanel(name, status, t));

            panel.SetValuesWithoutNotifying(
                component.IsStatusSelfHiding(status),
                templateList.Contains(status),
                service.IsStatusHiddenGlobally(status));

            panel.OnHidingRequested += (v, type) => ChangeHidingStatus(status, v, type);
        }
    }

    void ChangeHidingStatus(string status, bool hide, StatusHidingChangeType type)
    {
        switch (type)
        {
            case StatusHidingChangeType.Self:
                component!.ToggleHiding(status, hide);
                break;
            case StatusHidingChangeType.Template:
                service.ToggleTemplateStatusHiding(component!.TemplateName, status, hide);
                break;
            case StatusHidingChangeType.Global:
                service.ToggleGlobalStatusHiding(status, hide);
                break;
        }
    }

    void OnHideCornerChanged(bool value) => service.ToggleHideCornerWarnings(value);

}
