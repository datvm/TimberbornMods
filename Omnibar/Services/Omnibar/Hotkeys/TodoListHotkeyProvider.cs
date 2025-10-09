namespace Omnibar.Services.Omnibar.Hotkeys;

public class TodoListHotkeyProvider(
    ILoc t,
    KeyBindingRegistry registry,
    InputBindingDescriber inputBindingDescriber
) : DefaultHotkeyProvider(t, registry, inputBindingDescriber)
{
    public const string CreateToDoId = "OmnibarCreateTodo";
    public const string AppendToDoId = "OmnibarAppendTodo";

    protected override IEnumerable<OmnibarActionKeybindingSpec> KeybindingIds { get; } = [
        new(CreateToDoId, "LV.OB.AddToTodoList"),
        new(AppendToDoId, "LV.OB.AppendToTodoList"),
    ];

    public KeyBinding CreateBinding { get; private set; } = null!;
    public KeyBinding AppendBinding { get; private set; } = null!;

    public override void Load()
    {
        base.Load();

        CreateBinding = Keybindings[0].KeyBinding;
        AppendBinding = Keybindings[1].KeyBinding;
    }

    public override IOmnibarHotkeyAction? GetAction(IOmnibarItem item)
    {
        return item is IOmnibarItemWithTodoList tdItem && tdItem.CanAddToTodoList
            ? new TodoListHotkeyAction(tdItem, Hotkeys, this)
            : null;
    }
}

public class TodoListHotkeyAction(
    IOmnibarItemWithTodoList item,
    IReadOnlyList<string> hotkeys,
    TodoListHotkeyProvider provider
) : IOmnibarHotkeyAction
{

    public IReadOnlyList<string> HotkeyPrompts { get; } = hotkeys;

    public bool ProcessInput(InputModifiers modifiers)
    {
        var append = provider.AppendBinding.IsPressed(modifiers);

        if (append || provider.CreateBinding.IsPressed(modifiers))
        {
            item.AddToTodoList(append);
            return true;
        }

        return false;
    }
}
