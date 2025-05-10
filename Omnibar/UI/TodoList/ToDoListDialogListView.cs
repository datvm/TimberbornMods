namespace Omnibar.UI.TodoList;

public class ToDoListDialogListView : ListView
{
    const int ItemHeight = 32;

    ToDoListManager man = null!;
    VisualElementLoader loader = null!;

    public List<ToDoListEntry> Entries { get; private set; } = [];
    readonly List<ToDoListDialogListViewItem> items = [];

    public event Action<ToDoListEntry?> OnEntrySelected = null!;

    public ToDoListEntry? SelectedEntry => selectedItem as ToDoListEntry;

    public ToDoListDialogListView()
    {
        fixedItemHeight = ItemHeight;

        makeItem = MakeItem;
        bindItem = BindItem;
        unbindItem = UnbindItem;
    }

    public ToDoListDialogListView Init(ToDoListManager man, VisualElementLoader loader)
    {
        this.man = man;
        this.loader = loader;

        man.EntriesChanged += ReloadTodoList;
        man.EntryChanged += ReloadTodoEntry;

        return this;
    }

    private void ReloadTodoEntry(ToDoListEntry obj)
    {
        foreach (var item in items)
        {
            if (item.Entry == obj)
            {
                item.ShowItemContent();
            }
        }
    }

    public void ReloadTodoList(List<ToDoListEntry>? entries = null)
    {
        var currId = SelectedEntry?.Id;

        itemsSource = Entries = entries ?? man.Entries;

        SelectItem(currId ?? -1);
        OnEntrySelected(SelectedEntry);

        RefreshItems();

        if (Entries.Count == 0)
        {
            OnEntrySelected(null);
        }
    }

    public void SelectItem(int id)
    {
        if (Entries.Count == 0)
        {
            OnEntrySelected(null);
            return;
        }

        var itemIndex = Entries.FindIndex(q => q.Id == id);
        var finalIndex = selectedIndex = itemIndex == -1 ? 0 : itemIndex;
        OnEntrySelected(Entries[finalIndex]);
    }

    public void UpdateEntry()
    {

    }

    VisualElement MakeItem()
    {
        ToDoListDialogListViewItem result = new(loader);
        items.Add(result);

        result.OnClick += entry =>
        {
            if (entry is not null)
            {
                SelectItem(entry.Id);
            }
        };

        return result.Root;
    }

    void BindItem(VisualElement ve, int index)
    {
        ((ToDoListDialogListViewItem)ve.dataSource).SetItem(Entries[index]);
    }

    static void UnbindItem(VisualElement ve, int index)
    {
        ((ToDoListDialogListViewItem)ve.dataSource).UnsetItem();
    }

}

public class ToDoListDialogListViewItem
{
    public event Action<ToDoListEntry?> OnClick = null!;

    public ToDoListEntry? Entry { get; private set; }

    public VisualElement Root { get; private set; }

    readonly Label lblTitle;
    public ToDoListDialogListViewItem(VisualElementLoader loader)
    {
        Root = loader.LoadVisualElement("Options/ListViewItem");
        Root.dataSource = this;

        lblTitle = Root.Q<Label>("Text")
            .SetMaxSizePercent(100, null);
        lblTitle.style.textOverflow = TextOverflow.Ellipsis;
        lblTitle.style.whiteSpace = WhiteSpace.NoWrap;

        Root.RegisterCallback<MouseDownEvent>(ev =>
        {
            OnClick(Entry);
        });

    }

    public void SetItem(ToDoListEntry entry)
    {
        Entry = entry;
        ShowItemContent();
    }

    public void ShowItemContent()
    {
        if (Entry is null) { return; }

        lblTitle.text = Entry.Title.Strikethrough(Entry.Completed);
    }

    public void UnsetItem()
    {
        Entry = null;
    }

}
