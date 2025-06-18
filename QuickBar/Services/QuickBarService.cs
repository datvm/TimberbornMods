namespace QuickBar.Services;

public class QuickBarService(
    ISingletonLoader loader,
    QuickBarPersistentService persistent,
    IEnumerable<IQuickBarItemProvider> providers,
    EventBus eb
) : ILoadableSingleton, ISaveableSingleton, IPostLoadableSingleton
{
    static readonly ListKey<string> ItemsKey = new("Items");

    readonly IQuickBarItem?[] items = new IQuickBarItem[QuickBarModUtils.TotalQuickbarSlots];
    public IReadOnlyList<IQuickBarItem?> Items => items;
    public event Action<int, IQuickBarItem?>? ItemChanged;

    public void Load()
    {
        eb.Register(this);
    }

    public void PostLoad()
    {
        LoadSavedData();
    }

    void LoadSavedData()
    {
        if (!loader.TryGetSingleton(QuickBarModUtils.SaveKey, out var s)
            || !s.Has(ItemsKey)) { return; }

        var items = s.Get(ItemsKey).Select(persistent.Deserialize).ToArray();
        var instanceItems = this.items;

        var min = Math.Min(items.Length, instanceItems.Length);
        for (int i = 0; i < min; i++)
        {
            instanceItems[i] = items[i];
        }
    }

    public void Save(ISingletonSaver singletonSaver)
    {
        var s = singletonSaver.GetSingleton(QuickBarModUtils.SaveKey);
        s.Set(ItemsKey, [.. items.Select(persistent.Serialize)]);
    }

    public void Set(int slot, IQuickBarItem? item)
    {
        items[slot] = item;
        ItemChanged?.Invoke(slot, item);
    }

    public bool TryMakeItem(IOmnibarItem omnibarItem, [NotNullWhen(true)] out IQuickBarItem? quickBarItem)
    {
        quickBarItem = null;

        foreach (var p in providers)
        {
            if (p.TryCreateItem(omnibarItem, out quickBarItem))
            {
                return true;
            }
        }

        return false;
    }

    public void RevalidateItems()
    {
        for (int i = 0; i < items.Length; i++)
        {
            var item = items[i];
            if(item is not null && !item.IsStillValid())
            {
                Set(i, null);
            }
        }
    }

    [OnEvent]
    public void OnEntityDeleted(EntityDeletedEvent _)
    {
        RevalidateItems();
    }

}
