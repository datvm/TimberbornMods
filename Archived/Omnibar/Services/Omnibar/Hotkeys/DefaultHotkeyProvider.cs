namespace Omnibar.Services.Omnibar.Hotkeys;

public abstract class DefaultHotkeyProvider(
    ILoc t,
    KeyBindingRegistry registry,
    InputBindingDescriber inputBindingDescriber
) : IOmnibarHotkeyProvider, ILoadableSingleton
{

    protected abstract IEnumerable<OmnibarActionKeybindingSpec> KeybindingIds { get; }
    protected OmnibarActionKeybinding[] Keybindings { get; set; } = [];
    protected IReadOnlyList<string> Hotkeys { get; set; } = [];

    public virtual void Load()
    {
        Keybindings = [.. KeybindingIds
            .Select(q => new OmnibarActionKeybinding(registry.Get(q.KeybindingId), q.LocKey))];
        Hotkeys = [..Keybindings
            .Select(q => GetHotKey(q.KeyBinding, q.LocKey))
            .Where(q => q is not null)!];
    }

    string? GetHotKey(KeyBinding binding, string locKey)
    {
        var mainBinding = binding.PrimaryInputBinding ?? binding.SecondaryInputBinding;
        if (mainBinding is null) { return null; }

        var hotkey = inputBindingDescriber.GetInputBindingText(mainBinding);
        return t.GetOmnibarItemHotkey(hotkey, locKey);
    }

    public abstract IOmnibarHotkeyAction? GetAction(IOmnibarItem item);

}

public readonly record struct OmnibarActionKeybindingSpec(string KeybindingId, string LocKey);
public readonly record struct OmnibarActionKeybinding(KeyBinding KeyBinding, string LocKey);
