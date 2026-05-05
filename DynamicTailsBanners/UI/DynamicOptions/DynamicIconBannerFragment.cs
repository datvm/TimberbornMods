namespace DynamicTailsBanners.UI.DynamicOptions;

[MultiBind(typeof(IDecalOptionFragment))]
public class DynamicIconBannerFragment(
    DynamicIconBannerProvider p,
    ILoc t,
    EntityPickerPanel entityPickerPanel,
    ColorPickerPanel colorPickerPanel,
    UpdatableEntityStatService statService
) : VisualElement, IDecalOptionFragment
{
    public string Id => DynamicIconBannerProvider.Id;
    public bool Visible => this.IsDisplayed();

#nullable disable    
    ToggleGroup<IUpdatableEntityStat> grpStats;
    VisualElement chkPresentations;
#nullable enable

    DynamicDecalOption? curr;
    DynamicIconBannerOptions? currSettings;
    readonly Dictionary<string, ToggleGroupOption<IUpdatableEntityStat>> stats = [];

    public VisualElement InitializeFragment()
    {
        colorPickerPanel.OnColorChanged += OnColorPicked;
        Add(colorPickerPanel.SetMarginBottom(10));

        this.AddLabel(t.T("LV.DTB.SelectedBuilding"));
        entityPickerPanel.OnEntityChanged += OnEntityPicked;
        Add(entityPickerPanel.SetMarginBottom(10));

        this.AddLabel(t.T("LV.DTB.DisplayIcon"));
        chkPresentations = this.AddChild();

        foreach (var stat in statService.AllImageStats)
        {
            var chk = chkPresentations.AddToggle(t.T(stat.DisplayLoc));
            stats[stat.Id] = new(chk, stat);
            chk.SetDisplay(false);
        }

        grpStats = new(stats.Values);
        grpStats.OnValueChanged += OnStatChanged;

        return this;
    }

    void OnStatChanged(object sender, IUpdatableEntityStat e)
    {
        if (curr) { p.SetStat(curr!, e); }
    }

    void OnEntityPicked(object sender, (BaseComponent? source, EntityComponent?) e)
    {
        var (source, entity) = e;

        if (!source || source is not DynamicDecalOption opts) { return; }

        p.SetEntity(opts, entity);
    }

    void OnColorPicked(object sender, Color e)
    {
        if (currSettings is null) { return; }
        currSettings.Color = e;
        ReloadTexture();
    }

    void ReloadTexture()
    {
        if (!curr) { return; }

        curr!.RefreshDecalTexture();
    }

    public void ShowFragment(DecalSupplier decalSupplier)
    {
        curr = decalSupplier.GetComponent<DynamicDecalOption>();
        currSettings = curr.GetSettingsOrDefault<DynamicIconBannerOptions>();
        
        colorPickerPanel.SetColorWithoutNotifying(currSettings.Color);

        statService.TryGetStat(currSettings.StatId, out var stat);
        grpStats.SetValueWithoutNotify(stat);

        ShowBuilding(curr.Components[0]);

        this.SetDisplay(true);
    }

    public void ClearFragment()
    {
        this.SetDisplay(false);
        curr = null;
        currSettings = null;
        entityPickerPanel.ClearWithoutNotifying();
    }

    public void UpdateFragment() { }

    void ShowBuilding(EntityComponent? comp)
    {
        entityPickerPanel.SourceEntity = curr;
        entityPickerPanel.SetEntityWithoutNotifying(comp);

        if (!comp)
        {
            chkPresentations.SetDisplay(false);
            return;
        }

        var updatable = comp!.GetComponent<UpdatableEntityStatComponent>();
        foreach (var (chk, stat) in grpStats.Options)
        {
            chk.SetDisplay(stat.CanTrack(updatable));
        }

        chkPresentations.SetDisplay(true);
    }

}
