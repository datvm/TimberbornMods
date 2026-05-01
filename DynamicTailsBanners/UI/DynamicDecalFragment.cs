namespace DynamicTailsBanners.UI;

[BindSingleton]
public class DynamicDecalFragment(
    DecalSupplierFragment decalSupplierFragment,
    ILoc t,
    IEnumerable<IDecalOptionFragment> fragments,
#pragma warning disable CS9113 // For DI
    IEntityPanel _
#pragma warning restore CS9113 // Parameter is unread.
) : CollapsiblePanel, ILoadableSingleton, IUnloadableSingleton
{

    readonly FrozenDictionary<string, IDecalOptionFragment> fragments = fragments.ToFrozenDictionary(f => f.Id);
    public static DynamicDecalFragment? Instance { get; private set; }

    DecalSupplier? decalSupplier;
    string? currId;
    IDecalOptionFragment? currOption;

    VisualElement panelContainer = null!;

    public void Load()
    {
        Instance = this;
                    
        var userButtonsContainer = decalSupplierFragment._root.Q("UserPatternsLabel").parent;

        panelContainer = new NineSliceVisualElement().AddClasses("entity-sub-panel", "bg-sub-box--green");
        panelContainer.SetDisplay(false);
        InitializeFragments();

        panelContainer.Add(this);
        panelContainer.InsertSelfBefore(userButtonsContainer);
    }

    void InitializeFragments()
    {
        var parent = Container;

        foreach (var f in fragments.Values)
        {
            var el = f.InitializeFragment();
            el.SetDisplay(false);
            parent.Add(el);
        }
    }

    public void ShowFragment()
    {
        decalSupplier = decalSupplierFragment._decalSupplier;
        if (!decalSupplier) { return; }
        
        SetTitle(t.T("LV.DTB.OptionsTitle", t.TDecal(decalSupplier.Category)));
    }

    public void UpdateFragment()
    {
        if (!decalSupplier) { return; }

        var id = decalSupplier!.ActiveDecal.Id;
        if (id != currId)
        {
            currId = id;
            currOption?.ClearFragment();

            currOption = fragments.TryGetValue(id, out var option) ? option : null;
            if (currOption is not null)
            {
                currOption.ShowFragment(decalSupplier);
                panelContainer.SetDisplay(currOption.Visible);
            }
            else
            {
                panelContainer.SetDisplay(false);
            }
        }

        if (currOption is not null)
        {
            currOption.UpdateFragment();
            panelContainer.SetDisplay(currOption.Visible);
        }        
    }

    public void ClearFragment()
    {
        currOption?.ClearFragment();
        currOption = null;
        currId = null;
        decalSupplier = null;

        panelContainer.SetDisplay(false);
    }

    public void Unload() => Instance = null;

}
