namespace DirectionalDynamite.UI;

public class DirectionalDynamiteFragment(
    VisualElementInitializer veInit,
    DropdownItemsSetter dropdownItemsSetter,
    IAssetLoader assetLoader,
    DirectionalDynamiteService directionalDynamiteService,
    ILoc t
) : BaseEntityPanelFragment<DirectionalDynamiteComponent>, ILoadableSingleton
{

#nullable disable    
    Dropdown cboDirection;
    Sprite arrowIndicator;
    Toggle chkDoNotTrigger;
#nullable enable

    new IDirectionalDynamiteComponent? component;

    public void Load()
    {
        arrowIndicator = assetLoader.Load<Sprite>("Sprites/UI/chevron");
    }

    protected override void InitializePanel()
    {
        var row = panel.AddRow().SetMarginBottom(5).AlignItems();
        row.AddGameLabel(t.T("LV.DDy.Direction")).SetFlexShrink(0);

        cboDirection = row.AddDropdown()
            .AddChangeHandler(OnDirectionSelected)
            .SetFlexGrow(1);

        chkDoNotTrigger = panel.AddToggle(t.T("LV.DDy.DoNotTriggerNeighbor"), onValueChanged: OnDoNotTriggerChanged);

        panel.Initialize(veInit);

        cboDirection.SetItems(dropdownItemsSetter, directionalDynamiteService.DirectionNames);
    }

    static IDirectionalDynamiteComponent? TryGetComponent(BaseComponent entity)
    {
        if (MStarter.HasMacroManagement)
        {
            var mm = MMDirectionalDynamiteComponent.TryGetMM(entity);
            if (mm is not null)
            {
                return mm;
            }
        }

        var result = entity.GetComponentFast<DirectionalDynamiteComponent>();
        return result ? result : null;
    }

    public override void ShowFragment(BaseComponent entity)
    {
        // Do not call base, we have custom logic
        component = TryGetComponent(entity);
        if (component is null) { return; }

        cboDirection.SetSelectedItem(DirectionalDynamiteService.AllDirections.IndexOf(component.Direction));
        ShowDirection();

        chkDoNotTrigger.SetValueWithoutNotify(component.DoNotTriggerNeighbor);
        panel.Visible = true;
    }

    public override void ClearFragment()
    {
        HideDirection();
        component = null;
        base.ClearFragment();
    }

    void OnDirectionSelected(string? value, int index)
    {
        if (component is null) { return; }
        component.Direction = DirectionalDynamiteService.AllDirections[index];
        ShowDirection();
    }

    void ShowDirection() => component!.ShowIndicator(arrowIndicator);
    void HideDirection() => component?.HideIndicator();

    void OnDoNotTriggerChanged(bool value)
    {
        if (component is null) { return; }
        component.DoNotTriggerNeighbor = value;
    }

}