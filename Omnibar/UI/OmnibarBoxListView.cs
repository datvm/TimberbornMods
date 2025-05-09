namespace Omnibar.UI;

public class OmnibarBoxListView : ListView
{
    const int ItemHeight = 70;
    static readonly OmnibarFilteredItem[] Empty = [];

    readonly List<OmnibarListItem> listItems = [];
    List<OmnibarFilteredItem>? items;

    public Texture2D QuestionTexture { get; set; } = null!;

    public OmnibarFilteredItem? SelectingItem => items is null || selectedIndex == -1
        ? null
        : items[selectedIndex];

    public OmnibarBoxListView()
    {
        this.SetMargin(top: 30)
            .SetSize(height: 300)
            .SetFlexGrow(1)
            .AddClasses(UiCssClasses.ScrollGreenDecorated, "panel-list-view", "text--default");

        fixedItemHeight = ItemHeight;

        makeItem = MakeListItem;
        bindItem = BindListItem;
        unbindItem = UnbindListItem;

        selectedIndicesChanged += OnUserSelectedIndex;
    }

    private void OnUserSelectedIndex(IEnumerable<int> indices)
    {
        var index = indices.Any() ? indices.First() : -1;
        SetSelectingIndex(index, true);
    }

    OmnibarListItem MakeListItem()
    {
        OmnibarListItem result = new(QuestionTexture);
        listItems.Add(result);
        return result;
    }

    public void SetItems(List<OmnibarFilteredItem>? items)
    {
        this.items = items;
        selectedIndex = -1;

        if (items is null || items.Count == 0)
        {
            itemsSource = Empty;
            visible = false;
        }
        else
        {
            itemsSource = items;
            visible = true;
            SetSelectingIndex(0);
        }

        RefreshItems();
    }

    void BindListItem(VisualElement ve, int index)
    {
        if (items is null) { return; } // Should not happen

        var item = items[index];
        var el = (OmnibarListItem)ve;

        el.SetItem(index, item, selectedIndex);
    }

    void UnbindListItem(VisualElement ve, int index)
    {
        if (items is null || ve is not OmnibarListItem lstItem) { return; } // Should not happen

        lstItem.UnsetItem();
    }

    void SetSelectingIndex(int index, bool force = false)
    {
        if (!force && selectedIndex == index) { return; }

        selectedIndex = index;
        foreach (var item in listItems)
        {
            item.SetSelectedIndex(index);
        }

        ScrollToItem(index);
        RefreshItem(index);
    }

    public bool SelectItemWithKey(int delta)
    {
        if (!visible) { return false; }

        var expectingIndex = selectedIndex + delta;
        if (expectingIndex < 0 || expectingIndex >= itemsSource.Count)
        {
            // Still "swallow" the key input
            return true;
        }

        SetSelectingIndex(expectingIndex);
        return true;
    }

}
