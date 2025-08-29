namespace BuildingHP.UI;

public class RenovationListView : VisualElement
{

    public event Action<RenovationProviderItemModel>? RenovationSelected;

    RenovationListViewItem? selectingItem;
    public IRenovationProvider? SelectingItem => selectingItem?.Model.Provider;

    public RenovationListView Init(BuildingRenovationComponent comp, RenovationRegistry renovationRegistry, ILoc t)
    {
        var parent = this.AddScrollView();
        RenovationListViewItem? firstItem = null;

        foreach (var grp in renovationRegistry.OrderedGroups)
        {
            if (!renovationRegistry.ProviderGroups.TryGetValue(grp.Id, out var grpItems))
            {
                // No providers in this group
                continue;
            }

            var grpEl = new RenovationListViewGroup(grp);
            var items = grpItems
                .Select(q => new RenovationProviderItemModel(q, q.CanRenovate(comp)))
                .ToImmutableArray();

            var firstItemInGroup = grpEl.SetItems(items, OnRenovationUISelected);
            firstItem ??= firstItemInGroup;

            parent.Add(grpEl);
        }

        if (firstItem is null)
        {
            parent.AddGameLabel(t.T("LV.BHP.NoRenovation"));
        }
        else
        {
            OnRenovationUISelected(firstItem);
        }

        return this;
    }

    void OnRenovationUISelected(RenovationListViewItem item)
    {
        selectingItem?.Unselect();

        selectingItem = item;
        selectingItem.Select();

        RenovationSelected?.Invoke(item.Model);
    }

}

public class RenovationListViewGroup : CollapsiblePanel
{

    public RenovationListViewGroup(RenovationGroupSpec spec)
    {
        SetTitle(spec.Title.Value);
    }

    public RenovationListViewItem? SetItems(ImmutableArray<RenovationProviderItemModel> renovations, Action<RenovationListViewItem> onRenovationUISelected)
    {
        Container.Clear();

        if (renovations.Length == 0)
        {
            this.SetDisplay(false);
            return null;
        }

        RenovationListViewItem firstItem = null!;
        foreach (var r in renovations)
        {
            var item = new RenovationListViewItem(r, onRenovationUISelected);
            Container.Add(item);

            firstItem ??= item;
        }

        this.SetDisplay(true);
        return firstItem;
    }

}

public class RenovationListViewItem : NineSliceVisualElement
{

    readonly Label lbl;
    public RenovationProviderItemModel Model { get; }

    public RenovationListViewItem(RenovationProviderItemModel model, Action<RenovationListViewItem> callback)
    {
        Model = model;

        lbl = this.AddGameLabel(model.Provider.RenovationSpec.Title.Value)
            .SetPadding(left: 10, top: 10, bottom: 10);

        if (!model.IsAvailable)
        {
            lbl.style.color = Color.gray;
        }

        RegisterCallback<ClickEvent>(_ => callback(this));
    }

    public void Select()
    {
        lbl.style.unityFontStyleAndWeight = FontStyle.Bold;
    }

    public void Unselect()
    {
        lbl.style.unityFontStyleAndWeight = FontStyle.Normal;
    }

}

public readonly record struct RenovationProviderItemModel(IRenovationProvider Provider, string? NotAvailableReason)
{
    public bool IsAvailable => NotAvailableReason is null;
}
