namespace BuildingRenovations.UI;

[BindFragment]
public class BuildingRenovationFragment(
    ILoc t,
    IContainer container,
    RenovationDialogController renovationDialogController
) : BaseEntityPanelFragment<BuildingRenovationComponent>, IEntityFragmentOrder
{
#nullable disable
    Button btnRenovate;
    BuildingRenovationElement renoPanel;
    RenovationListElement renovationListPanel;
#nullable enable

    public int Order { get; } = -95;
    public VisualElement Fragment => panel;

    protected override void InitializePanel()
    {
        btnRenovate = panel
            .AddStretchedEntityFragmentButton(t.T("LV.BRe.Renovate"), onClick: OnRenovateRequested, color: EntityFragmentButtonColor.Red)
            .SetMarginBottom();

        renoPanel = panel.AddChild(() => container.GetInstance<BuildingRenovationElement>().Init())
            .SetMarginBottom();

        renovationListPanel = panel.AddChild(() => container.GetInstance<RenovationListElement>().Init());
    }

    public override void ShowFragment(BaseComponent entity)
    {
        base.ShowFragment(entity);
        if (!component) { return; }

        renoPanel.SetComponent(component);
        renovationListPanel.SetComponent(component!);
        UpdateFragment();
    }

    public override void UpdateFragment()
    {
        if (!component) { return; }

        if (!component!.IsFinished)
        {
            panel.SetDisplay(false);
            return;
        }

        panel.SetDisplay(true);
        renoPanel.Update();
        btnRenovate.SetDisplay(component.CanRenovate);
    }

    public override void ClearFragment()
    {
        base.ClearFragment();
        renoPanel.Unset();
        renovationListPanel.Unset();
    }

    async void OnRenovateRequested()
    {
        if (!component) { return; }
        await renovationDialogController.OpenDialogAsync(component!);
    }
}
