namespace BuildingRenovations.UI;

[BindTransient]
public class RenovationListView(
    ILoc t,
    RenovationRegistry registry,
    IContainer container
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

            grpEl.SetItems(items, OnRenovationUISelected, container);
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

