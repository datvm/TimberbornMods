
namespace QuickBar.Services;

public class OmnibarQuickbarProvider(
    ILoc t,
    KeyBindingRegistry registry,
    InputBindingDescriber inputBindingDescriber,
    QuickBarService service
) : DefaultHotkeyProvider(t, registry, inputBindingDescriber)
{
    readonly ILoc t = t;

    public ImmutableArray<KeyBinding> AllKeybindings => service.AllKeybindings;

    protected override IEnumerable<OmnibarActionKeybindingSpec> KeybindingIds { get; } = QuickBarService.AllHotkeyIds
        .Select(q => new OmnibarActionKeybindingSpec(q, "Core.OK"));

    public override void Load()
    {
        base.Load();
        Hotkeys = [t.GetOmnibarItemHotkey(t.T("LV.QkB.SetQuickbarHotkey"), "LV.QkB.SetQuickbar")];
    }

    public override IOmnibarHotkeyAction? GetAction(IOmnibarItem item) =>
        service.TryMakeItem(item, out var quickBarItem)
            ? new OmnibarQuickbarAction(quickBarItem, Hotkeys, this)
            : null;

    public void AddToQuickBar(int slot, IQuickBarItem item)
    {
        service.Set(slot, item);
    }

}

public class OmnibarQuickbarAction(
    IQuickBarItem item,
    IReadOnlyList<string> hotkeys,
    OmnibarQuickbarProvider provider
) : IOmnibarHotkeyAction
{
    public IReadOnlyList<string> HotkeyPrompts { get; } = hotkeys;

    public bool ProcessInput(InputModifiers modifiers)
    {
        if ((modifiers & InputModifiers.Shift) != InputModifiers.Shift) { return false; }

        for (int i = 0; i < provider.AllKeybindings.Length; i++)
        {
            var kb = provider.AllKeybindings[i];
            
            if (kb.IsPressed(modifiers, out _))
            {
                Debug.Log($"Adding item {item} to quickbar slot {i + 1}.");

                provider.AddToQuickBar(i, item);
                return true;
            }
        }

        return false;
    }

}
