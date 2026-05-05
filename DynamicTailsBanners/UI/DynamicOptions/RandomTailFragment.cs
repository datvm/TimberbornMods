namespace DynamicTailsBanners.UI.DynamicOptions;

[MultiBind(typeof(IDecalOptionFragment))]
public class RandomTailFragment(
    RandomTailProvider randomTailProvider,
    IContainer container,
    ILoc t
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
        var decals = await container.GetInstance<DecalPickerDialog>().PickMultipleAsync(DecalTypeEnum.Tails);
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
        var el = tailList.AddChild(container.GetInstance<DecalRow>).SetDecal(new(id, nameof(DecalTypeEnum.Tails)));
        el.OnRemoveRequested += Remove;
    }

    void Remove(object sender, DecalRow e)
    {
        randomTailProvider.DecalIds.Remove(e.Decal.Id);
        e.RemoveFromHierarchy();

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
        this.SetDisplay(false);
    }

    public void UpdateFragment()
    {
    }
}
