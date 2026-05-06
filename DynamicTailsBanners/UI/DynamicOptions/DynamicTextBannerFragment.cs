namespace DynamicTailsBanners.UI.DynamicOptions;

[MultiBind(typeof(IDecalOptionFragment))]
public class DynamicTextBannerFragment(
    ILoc t,
    UpdatableEntityStatService statService,
    VisualElementInitializer veInit,
    DropdownItemsSetter dropdownItemsSetter,
    DynamicBannerTextProvider p,
    EntityPickerPanel entityPickerPanel,
    ColorPickerPanel colorPickerPanel,
    DistrictCenterRegistry districtCenterRegistry
) : VisualElement, IDecalOptionFragment
{
    public string Id => DynamicBannerTextProvider.Id;
    public bool Visible => this.IsDisplayed();

#nullable disable
    VisualElement pnlPopulation;
    DropdownRow<PopulationCounterMode> cboPopulationModes;
    TextField txtContent;
    DropdownRow<int> cboFontSize;
    VisualElement pnlEntityPicker;
#nullable enable

    DropdownRow<IUpdatableEntityStat?> cboStats = null!;
    DropdownRow<DistrictCenter?> cboPopulationScope = null!;

    DynamicDecalOption? opts;
    DynamicBannerTextOptions? settings;

    public void ClearFragment()
    {
        opts = null;
        settings = null;
        entityPickerPanel.ClearWithoutNotifying();
        this.SetDisplay(false);
    }

    public VisualElement InitializeFragment()
    {
        Add(colorPickerPanel.SetMarginBottom(10));
        colorPickerPanel.OnColorChanged += OnColorChanged;

        cboFontSize = this.AddDropdownRow(
            [10, 20, 30, 40, 50, 60, 70, 80, 90, 100],
            s => s.ToString(),
            veInit, dropdownItemsSetter,
            t.T("LV.DTB.FontSize"),
            OnSizeChanged
        ).SetMarginBottom(10);

        cboStats = this.AddDropdownRow(
            [
                null,
                ..statService.AllStats.Where(s => s is not IImageStat),
            ],
            s => t.T(s?.DisplayLoc ?? "LV.DTB.TextProperty.Custom"),
            veInit, dropdownItemsSetter,
            t.T("LV.DTB.TextProperty"),
            OnStatSelected)
            .SetMarginBottom(10);

        pnlPopulation = this.AddChild().SetMarginBottom(10);
        cboPopulationModes = pnlPopulation.AddDropdownRow(
            PopulationStatService.PopulationCounterModes,
            m => m.T(t),
            veInit, dropdownItemsSetter,
            t.T("LV.DTB.PopulationMode"),
            OnPopulationModeSelected);
        cboPopulationScope = pnlPopulation.AddDropdownRow<DistrictCenter?>(
            t.T("LV.DTB.PopulationScope"),
            OnPopulationScopeSelected,
            veInit, dropdownItemsSetter)
            .SetMargin(top: 5);

        pnlEntityPicker = this.AddChild().SetMarginBottom(10);
        pnlEntityPicker.AddLabel(t.T("LV.DTB.SelectedBuilding"));
        pnlEntityPicker.Add(entityPickerPanel.SetMarginBottom(10));
        entityPickerPanel.OnEntityChanged += OnEntityPicked;
        

        var contentRow = this.AddRow().AlignItems();
        txtContent = contentRow.AddTextField("DynamicDecalTextContent", OnContentChanged).SetFlexGrow().SetMarginRight(5)
            .Initialize(veInit);

        return this;
    }

    void OnEntityPicked(object sender, (BaseComponent? Source, EntityComponent? Entity) e)
    {
        var (src, entity) = e;

        if (src is not DynamicDecalOption opts || !opts) { return; }
        p.SetEntity(opts, entity);
    }

    void OnColorChanged(object sender, Color e)
    {
        if (settings is null || !opts) { return; }

        settings.Color = e;
        opts!.RefreshDecalTexture();
    }

    public void ShowFragment(DecalSupplier decalSupplier)
    {
        opts = decalSupplier.GetDecalOptions();
        if (!opts)
        {
            ClearFragment();
            return;
        }

        settings = opts.GetSettings<DynamicBannerTextOptions>();
        if (settings is null)
        {
            ClearFragment();
            return;
        }

        cboFontSize.SetSelectedValueWithoutNotifying(settings.FontSize);

        var statId = settings.StatId;        
        if (statId is null || !statService.TryGetStat(settings.StatId ?? "", out var stat))
        {
            cboStats.SetSelectedIndexWithoutNotifying(0);
        }
        else
        {
            cboStats.SetSelectedValueWithoutNotifying(stat);
        }
        
        cboPopulationModes.SetSelectedValueWithoutNotifying(settings.PopulationMode);
        PopulateDistrictCenters();

        var dc = opts.Components[0]?.GetComponent<DistrictCenter>();
        if (dc)
        {
            cboPopulationScope.SetSelectedValueWithoutNotifying(dc);
        }
        else
        {
            cboPopulationScope.SetSelectedIndexWithoutNotifying(0);
        }

        colorPickerPanel.SetColorWithoutNotifying(settings.Color);

        entityPickerPanel.SourceEntity = opts;
        entityPickerPanel.SetEntityWithoutNotifying(opts.Components[0]);

        txtContent.SetValueWithoutNotify(settings.Content);

        this.SetDisplay(true);
        SetUIStates();
    }

    public void UpdateFragment()
    {
        if (settings is null) { return; }

        if (!settings.IsCustomText)
        {
            txtContent.SetValueWithoutNotify(settings.Content);
        }
    }

    void PopulateDistrictCenters()
    {
        cboPopulationScope.SetItems([
            null,
            ..districtCenterRegistry.AllDistrictCenters,
        ], dc => dc?.DistrictName ?? t.T("LV.DTB.Global"), true);
    }

    void OnStatSelected(IndexedDropdownRowItem<IUpdatableEntityStat?> v)
    {
        if (!opts) { return; }

        p.SetStat(opts!, v.Item.Value);
        SetUIStates();
    }

    void OnPopulationModeSelected(IndexedDropdownRowItem<PopulationCounterMode> e)
    {
        if (!opts || settings is null) { return; }

        p.ChangeSettings(opts!, () =>
        {
            settings.PopulationMode = e.Item.Value;
        });
    }

    void OnPopulationScopeSelected(IndexedDropdownRowItem<DistrictCenter?> e)
    {
        if (!opts || settings is null) { return; }

        var dc = e.Item.Value?.GetComponentOrNull<EntityComponent>();
        p.SetEntity(opts!, dc);
    }

    void SetUIStates()
    {
        if (settings is null) { return; }

        var isPopulation = settings.StatId == PopulationStat.StatId;
        var isCustomText = settings.IsCustomText;

        pnlPopulation.SetDisplay(isPopulation);
        txtContent.enabledSelf = isCustomText;
        pnlEntityPicker.SetDisplay(!isPopulation && !isCustomText);
    }

    void OnContentChanged(string v)
    {
        if (!opts) { return; }

        p.SetCustomText(opts!, v);
    }

    void OnSizeChanged(IndexedDropdownRowItem<int> e)
    {
        if (!opts) { return; }

        p.SetTextSize(opts!, e.Item.Value);
    }

}
