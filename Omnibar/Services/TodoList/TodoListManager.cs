namespace Omnibar.Services.TodoList;

public class TodoListManager(
    ISingletonLoader loader,
    OmnibarToolProvider omnibarToolProvider
) : ILoadableSingleton, ISaveableSingleton
{
    static readonly SingletonKey SaveKey = new(nameof(Omnibar));
    static readonly ListKey<string> EntryListKey = new("ToDoListEntries");
    static readonly PropertyKey<int> TodoListCurrentId = new("ToDoListId");

    readonly List<TodoListEntry> entries = [];
    int currId = 0;

    public event Action<TodoListEntry>? EntryChanged;
    public event Action<List<TodoListEntry>>? EntriesChanged;

    public List<TodoListEntry> Entries => [.. entries];

    public TodoListEntry Add(TodoListEntry entry)
    {
        entry.Id = ++currId;
        entries.Add(entry);
        Sort();

        return entry;
    }

    public void Remove(TodoListEntry entry)
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

        foreach (var entry in entries)
        {
            FillInBuildings(entry);
        }

        EntriesChanged?.Invoke(entries);
    }

    public void Load()
    {
        LoadSavedData();
    }

    public void OnEntryChanged(TodoListEntry entry)
    {
        FillInBuildings(entry);
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
                    .Select(TodoListEntry.Deserialize));
            Sort();
        }

        if (s.Has(TodoListCurrentId))
        {
            currId = s.Get(TodoListCurrentId);
        }
    }

    void FillInBuildings(TodoListEntry entry)
    {
        if (entry.Buildings.Count == 0) { return; }

        for (int i = 0; i < entry.Buildings.Count; i++)
        {
            var building = entry.Buildings[i];

            if (building.BuildingTool is not null) { continue; }

            if (omnibarToolProvider.BuildingTools.TryGetValue(building.Building, out var buildingSpec))
            {
                building.BuildingTool = buildingSpec;
            }
            else
            {
                Debug.LogWarning($"Could not find building spec for {building.Building}");
                entry.Buildings.RemoveAt(i);
                i--;
            }
        }
    }

    public void Save(ISingletonSaver singletonSaver)
    {
        var s = singletonSaver.GetSingleton(SaveKey);

        s.Set(EntryListKey, [.. entries.Select(q => q.Serialize())]);
        s.Set(TodoListCurrentId, currId);
    }

}