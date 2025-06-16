namespace Omnibar.UI.TodoList;

public class TodoListDialogListView : ListView
{
    const int ItemHeight = 32;

    TodoListManager man = null!;
    VisualElementLoader loader = null!;

    public List<TodoListEntry> Entries { get; private set; } = [];
    readonly List<TodoListDialogListViewItem> items = [];

    public event Action<TodoListEntry?> OnEntrySelected = null!;

    public TodoListEntry? SelectedEntry => selectedItem as TodoListEntry;

    public TodoListDialogListView()
    {
        fixedItemHeight = ItemHeight;

        makeItem = MakeItem;
        bindItem = BindItem;
        unbindItem = UnbindItem;

        selectionChanged += OnItemSelected;
    }

    private void OnItemSelected(IEnumerable<object> obj)
    {
        var entry = obj.FirstOrDefault() as TodoListEntry;
        OnEntrySelected(entry);
    }

    public TodoListDialogListView Init(TodoListManager man, VisualElementLoader loader)
    {
        this.man = man;
        this.loader = loader;

        man.EntriesChanged += ReloadTodoList;
        man.EntryChanged += ReloadTodoEntry;

        return this;
    }

    private void ReloadTodoEntry(TodoListEntry obj)
    {
        foreach (var item in items)
        {
            if (item.Entry == obj)
            {
                item.ShowItemContent();
            }
        }
    }

    public void ReloadTodoList(List<TodoListEntry>? entries = null)
    {
        var currId = SelectedEntry?.Id;

        itemsSource = Entries = entries ?? man.Entries;
        RefreshItems();

        SelectItem(currId ?? -1);
        OnEntrySelected(SelectedEntry);
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

    VisualElement MakeItem()
    {
        TodoListDialogListViewItem result = new(loader);
        items.Add(result);
        return result.Root;
    }

    void BindItem(VisualElement ve, int index)
    {
        ((TodoListDialogListViewItem)ve.dataSource).SetItem(Entries[index]);
    }

    static void UnbindItem(VisualElement ve, int index)
    {
        ((TodoListDialogListViewItem)ve.dataSource).UnsetItem();
    }

}

public class TodoListDialogListViewItem
{
    public TodoListEntry? Entry { get; private set; }

    public VisualElement Root { get; private set; }

    readonly Label lblTitle;
    public TodoListDialogListViewItem(VisualElementLoader loader)
    {
        Root = loader.LoadVisualElement("Options/ListViewItem");
        Root.dataSource = this;

        lblTitle = Root.Q<Label>("Text")
            .SetMaxSizePercent(100, null);
        lblTitle.style.textOverflow = TextOverflow.Ellipsis;
        lblTitle.style.whiteSpace = WhiteSpace.NoWrap;
    }

    public void SetItem(TodoListEntry entry)
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
