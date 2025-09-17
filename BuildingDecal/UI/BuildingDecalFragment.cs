
namespace BuildingDecal.UI;

public class BuildingDecalFragment(
    BuildingDecalSelectDialog diag,
    GameSliderAlternativeManualValueDI sliderDI,
    DialogBoxShower diagShower,
    BuildingDecalPositionService buildingDecalPositionService,
    DropdownItemsSetter dropdownItemsSetter
) : BaseEntityPanelFragment<BuildingDecalComponent>
{
    readonly ILoc t = sliderDI.t;
    readonly VisualElementInitializer veInit = sliderDI.VeInit;

#nullable disable
    VisualElement decalPanelsContainer;
#nullable enable

    PrefabSpec? prefabSpec;
    ImmutableArray<DecalPositionSpec> templatePositions = [];
    Vector3Int buildingSize;

    protected override void InitializePanel()
    {
        panel.AddGameButton(t.T("LV.BDl.Add"), onClick: AddDecal, stretched: true)
            .SetPadding(5)
            .SetMarginBottom(10);
        decalPanelsContainer = panel.AddChild();
    }

    public override void ShowFragment(BaseComponent entity)
    {
        base.ShowFragment(entity);
        if (!component) { return; }

        prefabSpec = component.GetComponentFast<PrefabSpec>();
        if (!prefabSpec)
        {
            ClearFragment();
            return;
        }
        templatePositions = buildingDecalPositionService.GetPositionsForBuilding(prefabSpec.Name);
        buildingSize = component.BuildingSize;

        decalPanelsContainer.Clear();
        foreach (var item in component.DecalItems)
        {
            AppendDecalPanel(item);
        }
    }

    public override void ClearFragment()
    {
        decalPanelsContainer.Clear();
        base.ClearFragment();
        prefabSpec = null;
        templatePositions = [];
    }

    async void AddDecal()
    {
        if (!component) { return; }

        var decal = await diag.ShowPickerAsync();
        if (decal is null) { return; }

        var item = component.AddDecal(decal.Value);
        AppendDecalPanel(item)
            .ActivateFirstTemplate();
    }

    BuildingDecalItemPanel AppendDecalPanel(BuildingDecalItem item)
    {
        var panel = new BuildingDecalItemPanel(item, decalPanelsContainer.childCount + 1, sliderDI);
        panel.OnDecalRequested += OnItemDecalRequested;
        panel.OnDecalDeletionRequested += OnItemDeleteRequested;

        panel.Initialize(veInit);

        panel.SetTemplatePositions(templatePositions, buildingSize, dropdownItemsSetter);

        decalPanelsContainer.Add(panel);
        return panel;
    }

    private void OnItemDeleteRequested(object sender, EventArgs e)
    {
        diagShower.Create()
            .SetLocalizedMessage("LV.BDl.RemoveConfirm")
            .SetConfirmButton(() => DeleteDecal((BuildingDecalItemPanel)sender))
            .SetDefaultCancelButton()
            .Show();
    }

    void DeleteDecal(BuildingDecalItemPanel panel)
    {
        if (!component) { return; }

        component.RemoveDecal(panel.Item);
        panel.RemoveFromHierarchy();
    }

    private async void OnItemDecalRequested(object sender, EventArgs e)
    {
        var decal = await diag.ShowPickerAsync();
        if (decal is null) { return; }

        ((BuildingDecalItemPanel)sender).SetDecal(decal.Value);
    }
}
