namespace BuildingRenovations.UI;

[BindTransient]
public class RenovationListView(
    ILoc t,
    RenovationRegistry registry
) : VisualElement
{
    public event Action<RenovationListItemModel>? RenovationSelected;

    RenovationListViewItem? selectingItem;
    readonly List<RenovationListViewGroup> groups = [];

    public RenovationListView Init(BuildingRenovationComponent building)
    {
        Clear();
        groups.Clear();
        selectingItem = null;

        var parent = this.AddScrollView();

        foreach (var grp in registry.OrderedGroups)
        {
            if (!registry.Groups.TryGetValue(grp.Id, out var entries))
            {
                continue;
            }

            var grpEl = new RenovationListViewGroup(grp);
            var items = entries
                .Select(e =>
                {
                    // Hard filter (wrong building type, etc.): only shown when "Show unavailable" is on.
                    // Soft unavailability (prereqs, already active, ...): always listed, greyed.
                    var applicable = e.CanRenovate(building);
                    var reason = applicable
                        ? building.GetUnavailableReason(e)
                        : t.T("LV.BRe.NotApplicable");
                    return new RenovationListItemModel(e, applicable, reason);
                })
                .ToArray();

            grpEl.SetItems(items, OnRenovationUISelected);
            parent.Add(grpEl);
            groups.Add(grpEl);
        }

        // Prefer a startable entry; otherwise first soft-unavailable (still visible by default).
        var allItems = groups.SelectMany(g => g.Items);
        var firstItem = allItems.FirstOrDefault(i => i.Model.IsAvailable)
            ?? allItems.FirstOrDefault(i => i.Model.Applicable)
            ?? allItems.FirstOrDefault();

        if (firstItem is null)
        {
            parent.AddGameLabel(t.T("LV.BRe.NoRenovation"));
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
    public IReadOnlyList<RenovationListViewItem> Items => items;
    public RenovationListViewItem? FirstItem => items.FirstOrDefault();

    public RenovationListViewGroup(RenovationGroupSpec spec)
    {
        SetTitle(spec.Title.Value);
    }

    public void SetItems(IReadOnlyCollection<RenovationListItemModel> renovations, Action<RenovationListViewItem> onSelected)
    {
        Container.Clear();
        items.Clear();

        foreach (var r in renovations)
        {
            var item = new RenovationListViewItem(r, onSelected);
            Container.Add(item);
            items.Add(item);
        }

        this.SetDisplay(renovations.Count > 0);
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
    public RenovationListItemModel Model { get; }

    public RenovationListViewItem(RenovationListItemModel model, Action<RenovationListViewItem> callback)
    {
        Model = model;

        lbl = this.AddGameLabel(model.Renovation.Spec.Title.Value)
            .SetPadding(left: 10, top: 10, bottom: 10);

        if (!model.IsAvailable)
        {
            lbl.style.color = Color.gray;
        }

        RegisterCallback<ClickEvent>(_ => callback(this));
    }

    public void Select() => lbl.style.unityFontStyleAndWeight = FontStyle.Bold;
    public void Unselect() => lbl.style.unityFontStyleAndWeight = FontStyle.Normal;

    public bool Filter(RenovationDialogFilter filter)
    {
        var keyword = filter.Keyword;
        var match = string.IsNullOrEmpty(keyword)
            || Model.Renovation.Spec.Title.Value.Contains(keyword, StringComparison.OrdinalIgnoreCase);

        // Soft-unavailable (Applicable but has a reason) always stays listed.
        // Hard-unavailable (!Applicable) only when the toggle is on.
        match = match && (Model.Applicable || filter.ShowUnavailables);

        this.SetDisplay(match);
        return match;
    }
}

/// <param name="Applicable"><see cref="RenovationBase.CanRenovate"/> — hard filter.</param>
/// <param name="NotAvailableReason">Soft unavailability reason, or not-applicable text when hard-filtered.</param>
public readonly record struct RenovationListItemModel(
    RenovationBase Renovation,
    bool Applicable,
    string? NotAvailableReason
)
{
    public bool IsAvailable => Applicable && NotAvailableReason is null;
}
