namespace BuildingRenovations.UI;

public class RenovationListViewGroup : CollapsiblePanel
{
    readonly List<RenovationListViewItem> items = [];
    public IReadOnlyList<RenovationListViewItem> Items => items;
    public RenovationListViewItem? FirstItem => items.FirstOrDefault();

    public RenovationListViewGroup(RenovationGroupSpec spec)
    {
        SetTitle(spec.Title.Value);
    }

    public void SetItems(IReadOnlyCollection<RenovationListItemModel> renovations, Action<RenovationListViewItem> onSelected, IContainer container)
    {
        Container.Clear();
        items.Clear();

        foreach (var r in renovations)
        {
            var item = new RenovationListViewItem(r, onSelected, container);
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
