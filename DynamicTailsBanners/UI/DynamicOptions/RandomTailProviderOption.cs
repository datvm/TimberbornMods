namespace DynamicTailsBanners.UI.DynamicOptions;

[MultiBind(typeof(IDecalOptionFragment))]
public class RandomTailProviderOption(
    RandomTailProvider randomTailProvider,
    IContainer container,
    ILoc t,
    IDecalService decalService
) : VisualElement, IDecalOptionFragment
{
    public string Id => RandomTailProvider.Id;
    public bool Visible => this.IsDisplayed();

#nullable disable
    Toggle chkSameForAll;
    VisualElement tailList;
#nullable enable

    public VisualElement InitializeFragment()
    {
        chkSameForAll = this.AddToggle(t.T("LV.DTB.SameForAll"), onValueChanged: OnSameForAllChanged).SetMarginBottom(10);

        var listHeader = this.AddRow().SetMarginBottom(5);
        listHeader.AddLabel(t.T("LV.DTB.RandomList")).SetFlexGrow();
        listHeader.AddGameButtonPadded(t.T("LV.DTB.Add"), Add);

        tailList = this.AddScrollView().SetMaxHeight(300);

        return this;
    }

    void OnSameForAllChanged(bool v)
    {
        randomTailProvider.SameForAll = v;
        randomTailProvider.Randomize();
    }

    async void Add()
    {
        var decals = await container.GetInstance<DecalPickerDialog>().PickMultipleAsync(nameof(DecalTypeEnum.Tails));
        if (decals is null || decals.Length == 0) { return; }

        var set = randomTailProvider.DecalIds.ToHashSet();
        var adding = decals.Select(d => d.Id).Where(set.Add).ToArray();
        if (adding.Length == 0) { return; }

        foreach (var id in adding)
        {
            randomTailProvider.DecalIds.Add(id);
            AddDecalUI(id);
        }

        randomTailProvider.Randomize();
    }

    void RefreshList()
    {
        tailList.Clear();
        foreach (var id in randomTailProvider.DecalIds)
        {
            AddDecalUI(id);
        }
    }

    void AddDecalUI(string id)
    {
        var el = tailList.AddRow().AlignItems();

        var texture = decalService.GetDecalTexture(new(id, nameof(DecalTypeEnum.Tails)));
        el.AddIconSpan(texture, postfixText: id, size: 20).SetFlexGrow().SetFlexShrink().SetMarginRight(5);
        el.AddGameButtonPadded(t.T("LV.DTB.Remove"), onClick: () => Remove(el, id));
    }

    void Remove(VisualElement el, string id)
    {
        randomTailProvider.DecalIds.Remove(id);
        el.RemoveFromHierarchy();

        randomTailProvider.Randomize();
    }

    public void ShowFragment(DecalSupplier decalSupplier)
    {
        RefreshList();
        chkSameForAll.SetValueWithoutNotify(randomTailProvider.SameForAll);

        this.SetDisplay(true);
    }

    public void ClearFragment()
    {
        tailList.Clear();
    }

    public void UpdateFragment()
    {
    }
}
