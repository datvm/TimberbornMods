namespace Omnibar.Services;

public class ToDoListManager(
    ISingletonLoader loader
) : ILoadableSingleton, ISaveableSingleton
{
    static readonly SingletonKey SaveKey = new(nameof(Omnibar));
    static readonly ListKey<string> EntryListKey = new("Entries");

    readonly List<ToDoListEntry> entries = [];

    public void Load()
    {
        LoadSavedData();
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
        }
    }

    public void Save(ISingletonSaver singletonSaver)
    {
        var s = singletonSaver.GetSingleton(SaveKey);

        s.Set(EntryListKey, [.. entries.Select(q => q.Serialize())]);
    }

}