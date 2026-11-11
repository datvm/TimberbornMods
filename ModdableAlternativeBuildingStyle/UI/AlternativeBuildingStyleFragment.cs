namespace ModdableAlternativeBuildingStyle.UI;

[BindFragment]
public class AlternativeBuildingStyleFragment(
    ILoc t,
    VisualElementInitializer veInit,
    DropdownItemsSetter dropdownItemsSetter
) : BaseEntityPanelFragment<AlternativeBuildingStyleComponent>
{
    DropdownRow<string?> cboStyles = null!;
    
    protected override void InitializePanel()
    {
        cboStyles = panel.AddDropdownRow<string?>(t.T("LV.MABS.BuildingStyle"), OnStyleChanged, veInit, dropdownItemsSetter);
    }

    void OnStyleChanged(IndexedDropdownRowItem<string?> styleName)
    {
        if (!component) { return; }

        component!.TrySetStyle(styleName.Item.Value);
    }

    public override void ShowFragment(BaseComponent entity)
    {
        base.ShowFragment(entity);

        if (!component) { return; }
        if (!component!.HasAlternativeStyles)
        {
            ClearFragment();
            return;
        }

        cboStyles.SetItems([null, .. component.AlternativeStyles], s => s ?? t.T("LV.MABS.Original"));
        cboStyles.SetSelectedValueWithoutNotifying(component.AlternativeStyle);
    }

}
