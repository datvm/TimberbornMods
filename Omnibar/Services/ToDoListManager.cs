namespace Omnibar.Services;

public class ToDoListManager(
    ISingletonLoader loader
) : ILoadableSingleton, ISaveableSingleton
{
    static readonly SingletonKey SaveKey = new(nameof(Omnibar));
    static readonly ListKey<string> EntryListKey = new("ToDoListEntries");
    static readonly PropertyKey<int> TodoListCurrentId = new("ToDoListId");

    readonly List<ToDoListEntry> entries = [];
    int currId = 0;

    public event Action<ToDoListEntry>? EntryChanged;
    public event Action<List<ToDoListEntry>>? EntriesChanged;

    public List<ToDoListEntry> Entries => [..entries];

    public ToDoListEntry Add(ToDoListEntry entry)
    {
        entry.Id = ++currId;
        entries.Add(entry);
        Sort();

        return entry;
    }

    public void Remove(ToDoListEntry entry)
    {
        entries.Remove(entry);
        EntriesChanged?.Invoke(entries);
    }

    public void ClearCompleted()
    {
        entries.RemoveAll(q => q.Completed);
        EntriesChanged?.Invoke(entries);
    }

    public void Sort()
    {
        entries.Sort(static (a, b) =>
        {
            var completed = a.Completed.CompareTo(b.Completed);
            return completed == 0 ? a.Id.CompareTo(b.Id) : completed;
        });

        EntriesChanged?.Invoke(entries);
    }

    public void Load()
    {
        LoadSavedData();
    }

    public void OnEntryChanged(ToDoListEntry entry)
    {
        EntryChanged?.Invoke(entry);
    }

    void LoadSavedData()
    {
        if (!loader.TryGetSingleton(SaveKey, out var s)) { return; }

        if (s.Has(EntryListKey))
        {
            entries.Clear();
            entries.AddRange(
                s
                    .Get(EntryListKey)
                    .Select(ToDoListEntry.Deserialize));
            Sort();
        }

        if (s.Has(TodoListCurrentId))
        {
            currId = s.Get(TodoListCurrentId);
        }
    }

    public void Save(ISingletonSaver singletonSaver)
    {
        var s = singletonSaver.GetSingleton(SaveKey);

        s.Set(EntryListKey, [.. entries.Select(q => q.Serialize())]);
        s.Set(TodoListCurrentId, currId);
    }

}