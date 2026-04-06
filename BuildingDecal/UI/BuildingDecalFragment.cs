
namespace BuildingDecal.UI;

public class BuildingDecalFragment(
    BuildingDecalSelectDialog diag,
    GameSliderAlternativeManualValueDI sliderDI,
    DialogBoxShower diagShower,
    BuildingDecalPositionService buildingDecalPositionService,
    DropdownItemsSetter dropdownItemsSetter,
    BuildingDecalClipboard buildingDecalClipboard
) : BaseEntityPanelFragment<BuildingDecalComponent>
{
    readonly ILoc t = sliderDI.t;
    readonly VisualElementInitializer veInit = sliderDI.VeInit;

#nullable disable
    VisualElement decalPanelsContainer;
    Button btnPaste;
#nullable enable

    TemplateSpec? templateSpec;
    ImmutableArray<DecalPositionSpec> templatePositions = [];
    Vector3Int buildingSize;

    protected override void InitializePanel()
    {
        var buttons = panel.AddRow().AlignItems().SetMarginBottom(10);

        buttons.AddGameButton(t.T("LV.BDl.Add"), onClick: AddDecal)
            .SetPadding(5)
            .SetFlexGrow(1);
        btnPaste = buttons.AddGameButton(t.T("LV.BDl.PasteButton"), onClick: PasteDecal)
            .SetPadding(5)
            .SetFlexGrow(1);
        btnPaste.enabledSelf = false;

        decalPanelsContainer = panel.AddChild();
        buildingDecalClipboard.OnClipboardChanged += OnClipboardChanged;
    }

    void OnClipboardChanged()
    {
        btnPaste.enabledSelf = buildingDecalClipboard.CanPaste;
    }

    public override void ShowFragment(BaseComponent entity)
    {
        base.ShowFragment(entity);
        if (!component) { return; }

        templateSpec = component!.GetComponent<TemplateSpec>();
        if (templateSpec is null)
        {
            ClearFragment();
            return;
        }
        templatePositions = buildingDecalPositionService.GetPositionsForBuilding(templateSpec.TemplateName);
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
        templateSpec = null;
        templatePositions = [];
    }

    async void AddDecal()
    {
        if (!component) { return; }

        var decal = await diag.ShowPickerAsync();
        if (decal is null) { return; }

        var item = component!.AddDecal(decal.Value);
        AppendDecalPanel(item)
            .ActivateFirstTemplate();
    }

    BuildingDecalItemPanel AppendDecalPanel(BuildingDecalItem item)
    {
        var panel = new BuildingDecalItemPanel(item, decalPanelsContainer.childCount + 1, sliderDI);
        panel.OnDecalRequested += OnItemDecalRequested;
        panel.OnDecalDeletionRequested += OnItemDeleteRequested;
        panel.OnDecalCopyRequested += OnDecalCopyRequested;

        panel.Initialize(veInit);

        panel.SetTemplatePositions(templatePositions, buildingSize, dropdownItemsSetter);

        decalPanelsContainer.Add(panel);
        return panel;
    }

    void OnDecalCopyRequested(object sender, EventArgs e)
    {
        buildingDecalClipboard.Copy(((BuildingDecalItemPanel)sender).Item);
    }

    void PasteDecal()
    {
        if (!component) { return; }

        var item = buildingDecalClipboard.Paste(component!);
        if (item is not null)
        {
            AppendDecalPanel(item);
        }
    }

    void OnItemDeleteRequested(object sender, EventArgs e)
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

        component!.RemoveDecal(panel.Item);
        panel.RemoveFromHierarchy();
    }

    async void OnItemDecalRequested(object sender, EventArgs e)
    {
        var decal = await diag.ShowPickerAsync();
        if (decal is null) { return; }

        ((BuildingDecalItemPanel)sender).SetDecal(decal.Value);
    }

}
