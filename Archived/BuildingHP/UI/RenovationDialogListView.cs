namespace BuildingHP.UI;

public class RenovationListView : VisualElement
{

    public event Action<RenovationProviderItemModel>? RenovationSelected;

    RenovationListViewItem? selectingItem;
    public IRenovationProvider? SelectingItem => selectingItem?.Model.Provider;

    readonly List<RenovationListViewGroup> groups = [];

    public RenovationListView Init(BuildingRenovationComponent comp, RenovationRegistry renovationRegistry, ILoc t)
    {
        var parent = this.AddScrollView();

        groups.Clear();
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
                .ToArray();

            grpEl.SetItems(items, OnRenovationUISelected);
            
            parent.Add(grpEl);
            groups.Add(grpEl);
        }

        var firstItem = groups.FirstOrDefault()?.FirstItem;
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

    public void Filter(RenovationDialogFilter filter)
    {
        foreach (var group in groups)
        {
            group.Filter(filter);
        }
    }

}

public class RenovationListViewGroup : CollapsiblePanel
{

    readonly List<RenovationListViewItem> items = [];
    public RenovationListViewItem? FirstItem => items.FirstOrDefault();

    public RenovationListViewGroup(RenovationGroupSpec spec)
    {
        SetTitle(spec.Title.Value);
    }

    public void SetItems(IReadOnlyCollection<RenovationProviderItemModel> renovations, Action<RenovationListViewItem> onRenovationUISelected)
    {
        Container.Clear();
        items.Clear();

        if (renovations.Count == 0)
        {
            this.SetDisplay(false);
        }

        foreach (var r in renovations)
        {
            var item = new RenovationListViewItem(r, onRenovationUISelected);
            Container.Add(item);

            items.Add(item);
        }

        this.SetDisplay(true);
    }

    public void Filter(RenovationDialogFilter filter)
    {
        var hasMatch = false;

        foreach (var item in items)
        {
            if (item.Filter(filter))
            {
                hasMatch = true;
            }
        }

        this.SetDisplay(hasMatch);
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

    public bool Filter(RenovationDialogFilter filter)
    {
        var match = filter.Keyword is null || Model.Provider.RenovationSpec.Title.Value.Contains(filter.Keyword, StringComparison.OrdinalIgnoreCase);
        match = match && (Model.IsAvailable || filter.ShowUnavailables);

        this.SetDisplay(match);
        return match;
    }

}

public readonly record struct RenovationProviderItemModel(IRenovationProvider Provider, string? NotAvailableReason)
{
    public bool IsAvailable => NotAvailableReason is null;
}
