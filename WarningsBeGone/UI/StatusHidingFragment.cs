namespace WarningsBeGone.UI;

public class StatusHidingFragment(
    ILoc t,
    StatusHidingService repo
) : BaseEntityPanelFragment<StatusHidingComponent>
{

    VisualElement list = null!;

    protected override void InitializePanel()
    {
        list = panel.AddChild().SetPadding(top: 20);
    }

    public override void ShowFragment(BaseComponent entity)
    {
        base.ShowFragment(entity);
        if (component is null) { return; }

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
        var templateList = repo.GetHidingStatusesForTemplate(component.TemplateName);

        foreach (var status in statuses)
        {
            var panel = list.AddChild(() => new SingleStatusPanel(name, status, t));

            panel.SetValuesWithoutNotifying(
                component.IsStatusSelfHiding(status),
                templateList.Contains(status),
                repo.IsStatusHiddenGlobally(status));

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
                repo.ToggleTemplateStatusHiding(component!.TemplateName, status, hide);
                break;
            case StatusHidingChangeType.Global:
                repo.ToggleGlobalStatusHiding(status, hide);
                break;
        }
    }
}
