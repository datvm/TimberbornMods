namespace DynamicTailsBanners.UI.DynamicOptions;

[MultiBind(typeof(IDecalOptionFragment))]
public class RandomBannerFragment(
    RandomBannerProvider p,
    ILoc t,
    IContainer container
) : VisualElement, IDecalOptionFragment
{
    public string Id => RandomBannerProvider.Id;
    public bool Visible => this.IsDisplayed();

#nullable disable
    VisualElement decalList;
    Button btnRandomize;
#nullable enable

    DynamicBuildingDecal? curr;
    RandomBannerProviderOptions? opts;

    public void ClearFragment()
    {
        decalList.Clear();
        curr = null;
        opts = null;
        this.SetDisplay(false);
    }

    public VisualElement InitializeFragment()
    {
        var buttons = this.AddRow().AlignItems().SetMarginBottom(10);

        buttons.AddGameButtonPadded(t.T("LV.DTB.Add"), Add).SetMarginRight(5).SetFlexGrow();
        btnRandomize = buttons.AddGameButtonPadded(t.T("LV.DTB.Randomize"), Randomize).SetFlexGrow();

        decalList = this.AddScrollView().SetMaxHeight(300);

        return this;
    }

    void Randomize()
    {
        if (!curr) { return; }
        p.Rerandomize(curr!);
    }

    public void ShowFragment(DecalSupplier decalSupplier)
    {
        curr = decalSupplier.GetComponent<DynamicBuildingDecal>();
        opts = curr.Options.GetSettingsOrDefault<RandomBannerProviderOptions>();
        RefreshList();

        this.SetDisplay(true);
    }

    public void UpdateFragment() { }

    async void Add()
    {
        if (!curr || opts is null) { return; } 

        var decals = await container.GetInstance<DecalPickerDialog>().PickMultipleAsync(DecalTypeEnum.Banners);
        if (decals is null || decals.Length == 0) { return; }

        var set = opts.BannerIds.ToHashSet();
        var adding = decals.Select(d => d.Id).Where(set.Add).ToArray();
        if (adding.Length == 0) { return; }

        foreach (var id in adding)
        {
            opts.BannerIds.Add(id);
            AddDecalUI(id);
        }

        Randomize();

        SetEnabledUI();
    }

    void RefreshList()
    {
        decalList.Clear();
        if (opts is null) { return; }

        foreach (var id in opts.BannerIds)
        {
            AddDecalUI(id);
        }

        SetEnabledUI();
    }

    void AddDecalUI(string id)
    {
        var el = decalList.AddChild(container.GetInstance<DecalRow>).SetDecal(new(id, nameof(DecalTypeEnum.Banners)));
        el.OnRemoveRequested += OnRemove;
    }

    void OnRemove(object sender, DecalRow e)
    {
        if (opts is null) { return; }

        var id = e.Decal.Id;
        opts.BannerIds.Remove(id);

        e.RemoveFromHierarchy();
        SetEnabledUI();

        Randomize();
    }

    void SetEnabledUI() => btnRandomize.enabledSelf = decalList.childCount > 0;

}
