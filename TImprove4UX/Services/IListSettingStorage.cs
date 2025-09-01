namespace TImprove4UX.Services;

public interface IListSettingStorage
{

    IEnumerable<string> Items { get; }
    bool Contains(string item);

    void Save(ISingletonSaver saver);
    void Load();

    void Add(string item);
    void Remove(string item);
    void ClearAndImport(IEnumerable<string> items);

}

public class PerSaveListSettingStorage(string saveKey, ISingletonLoader loader) : IListSettingStorage
{
    static readonly ListKey<string> ItemsKey = new("Items");
    readonly SingletonKey SaveKey = new(saveKey);

    readonly HashSet<string> items = [];
    public IEnumerable<string> Items => items;
    public bool Contains(string item) => items.Contains(item);

    public void Add(string item) => items.Add(item);

    public void ClearAndImport(IEnumerable<string> items)
    {
        this.items.Clear();
        this.items.UnionWith(items);
    }

    public void Remove(string item) => items.Remove(item);

    public void Load()
    {
        if (!loader.TryGetSingleton(SaveKey, out var s)) { return; }

        if (s.Has(ItemsKey))
        {
            ClearAndImport(s.Get(ItemsKey));
        }
    }

    public void Save(ISingletonSaver saver)
    {
        var s = saver.GetSingleton(SaveKey);
        s.Set(ItemsKey, [.. Items]);
    }
}

public class GlobalListSettingStorage(string saveKey) : IListSettingStorage
{
    readonly HashSet<string> items = [];
    public IEnumerable<string> Items => items;
    public bool Contains(string item) => items.Contains(item);

    public void Add(string item)
    {
        items.Add(item);
        Save();
    }

    public void ClearAndImport(IEnumerable<string> items)
    {
        this.items.Clear();
        this.items.UnionWith(items);
        Save();
    }

    public void Remove(string item)
    {
        items.Remove(item);
        Save();
    }

    public void Load()
    {
        var list = PlayerPrefs.GetString(saveKey, "");
        if (string.IsNullOrEmpty(list)) { return; }

        ClearAndImport(list.Split(';', StringSplitOptions.RemoveEmptyEntries));
    }

    public void Save(ISingletonSaver _) => Save();

    void Save()
    {
        var list = string.Join(";", Items);
        PlayerPrefs.SetString(saveKey, list);
    }
}