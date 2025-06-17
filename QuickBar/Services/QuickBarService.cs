namespace QuickBar.Services;

public class QuickBarService(
    ISingletonLoader loader,
    KeyBindingRegistry keyBindingRegistry,
    QuickBarPersistentService persistent,
    IEnumerable<IQuickBarItemProvider> providers,
    InputBindingDescriber inputBindingDescriber,
    InputService inputService
) : ILoadableSingleton, ISaveableSingleton, IInputProcessor, IUnloadableSingleton
{

    static readonly ListKey<string> ItemsKey = new("Items");

    // Hotkeys
    public static readonly ImmutableArray<string> AllHotkeyIds = [..
        Enumerable.Range(1, QuickBarModUtils.TotalQuickbarSlots)
        .Select(i => string.Format(QuickBarModUtils.QuickbarHotKeyId, i))];
    
    public ImmutableArray<KeyBinding> AllKeybindings { get; private set; } = [];

    // Items
    readonly IQuickBarItem?[] items = new IQuickBarItem[QuickBarModUtils.TotalQuickbarSlots];
    public IReadOnlyList<IQuickBarItem?> Items => items;
    public event Action<int, IQuickBarItem?>? ItemChanged;

    public void Load()
    {
        LoadSavedData();
        AllKeybindings = [.. AllHotkeyIds.Select(q => keyBindingRegistry.Get(q))];

        inputService.AddInputProcessor(this);
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

    public string? GetShortcutText(int slotId)
    {
        var binding = AllKeybindings[slotId];
        var main = binding.PrimaryInputBinding ?? binding.SecondaryInputBinding;

        return main is null ? null : inputBindingDescriber.GetInputBindingText(main);
    }

    public bool ProcessInput()
    {
        for (int i = 0; i < AllHotkeyIds.Length; i++)
        {
            if (inputService.IsKeyDown(AllHotkeyIds[i]))
            {
                var item = Items[i];
                item?.Activate();

                return true;
            }
        }

        return false;
    }

    public void Unload()
    {
        inputService.RemoveInputProcessor(this);
    }
}
