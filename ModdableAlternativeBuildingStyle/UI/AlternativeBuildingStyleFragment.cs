namespace ModdableAlternativeBuildingStyle.UI;

[BindFragment]
public class AlternativeBuildingStyleFragment(
    ILoc t,
    VisualElementInitializer veInit,
    DropdownItemsSetter dropdownItemsSetter
) : BaseEntityPanelFragment<AlternativeBuildingStyleComponent>
{
    DropdownRow<string> cboStyles = null!;

    protected override void InitializePanel()
    {
        cboStyles = panel.AddDropdownRow<string>(t.T("LV.MABS.BuildingStyle"), OnStyleChanged, veInit, dropdownItemsSetter);
    }

    void OnStyleChanged(IndexedDropdownRowItem<string> styleName)
    {
        if (!component) { return; }

        var value = styleName.Item.Value;
        component!.TrySetStyle(value.Length == 0 ? null : value);
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

        cboStyles.SetItems(
            ["", .. component.AlternativeStyles],
            s => s.Length == 0 ? t.T("LV.MABS.Original") : s);
        cboStyles.SetSelectedValueWithoutNotifying(component.AlternativeStyle ?? "");
    }

}
