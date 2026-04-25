namespace RadialToolbar.Services;

[BindSingleton(Contexts = BindAttributeContext.NonMenu)]
public class RadialQuickSlotService(
    ISingletonLoader loader,
    BottomBarButtonLookupService lookupService
) : ISaveableSingleton, ILoadableSingleton
{
    static readonly SingletonKey SaveKey = new(nameof(RadialQuickSlotService));
    static readonly ListKey<string> IdsKey = new("SlotIds");
    static readonly ListKey<bool> PinnedKey = new("PinnedSlots");

    public const int SlotCount = 4;
    readonly string?[] slotIds = new string[SlotCount];
    readonly bool[] pinnedSlots = new bool[SlotCount];

    public event Action? OnChanged;

    public IReadOnlyList<QuickSlotItem> Slots => [.. slotIds
        .Zip(pinnedSlots, (id, pinned) => new QuickSlotItem(id, pinned))];

    public BottomBarButtonLookup? GetButtonAtSlot(int index)
    {
        var id = slotIds[index];
        if (id is null || !lookupService.TryGetById(id, out var lookup)) { return null; }

        return lookup;
    }

    public void Load()
    {
        if (!loader.TryGetSingleton(SaveKey, out var s)) { return; }

        var savedIds = s.Get(IdsKey);
        var savedPinned = s.Get(PinnedKey);

        for (int i = 0; i < SlotCount; i++)
        {
            if (savedIds.Count > i)
            {
                slotIds[i] = savedIds[i];
            }

            if (savedPinned.Count > i)
            {
                pinnedSlots[i] = savedPinned[i];
            }
        }
    }

    public void Save(ISingletonSaver singletonSaver)
    {
        var s = singletonSaver.GetSingleton(SaveKey);
        s.Set(IdsKey, slotIds);
        s.Set(PinnedKey, pinnedSlots);
    }

    public void TogglePin(int index, bool? value = null)
    {
        if (slotIds[index] is null) { return; } // Don't let player pin an empty slot

        value ??= !pinnedSlots[index];
        pinnedSlots[index] = value.Value;

        OnChanged?.Invoke();
    }

    public void Push(string id)
    {
        int? prevSlot = null;

        for (int i = SlotCount - 1; i >= 0; i--)
        {
            if (pinnedSlots[i]) { continue; }

            if (prevSlot is not null) // If no more slot, the oldest one will be overridden
            {
                slotIds[prevSlot.Value] = slotIds[i];
            }

            prevSlot = i;
        }

        if (prevSlot is not null) // null means all slots are pinned
        {
            slotIds[prevSlot.Value] = id;
            OnChanged?.Invoke();
        }
    }

}
