namespace QuickBar.Services;

public class QuickBarHotkeyService(
    KeyBindingRegistry keyBindingRegistry,
    InputService inputService,
    InputBindingDescriber inputBindingDescriber,
    QuickBarService service,
    EntitySelectionService selectionService,
    EntityQuickBarItemProvider itemProvider
) : ILoadableSingleton, IInputProcessor, IUnloadableSingleton
{
    public static readonly ImmutableArray<string> AllHotkeyIds = [..
        Enumerable.Range(1, QuickBarModUtils.TotalQuickbarSlots)
        .Select(i => string.Format(QuickBarModUtils.QuickbarHotKeyId, i))];

    public ImmutableArray<KeyBinding> AllKeybindings { get; private set; } = [];

    public void Load()
    {
        AllKeybindings = [.. AllHotkeyIds.Select(q => keyBindingRegistry.Get(q))];
        inputService.AddInputProcessor(this);
    }

    public void Unload()
    {
        inputService.RemoveInputProcessor(this);
    }

    public string? GetShortcutText(int slotId)
    {
        var binding = AllKeybindings[slotId];
        var main = binding.PrimaryInputBinding ?? binding.SecondaryInputBinding;

        return main is null ? null : inputBindingDescriber.GetInputBindingText(main);
    }

    public bool ProcessInput()
    {
        var items = service.Items;

        for (int i = 0; i < AllHotkeyIds.Length; i++)
        {
            if (inputService.IsKeyDown(AllHotkeyIds[i]))
            {
                if (Keyboard.current.shiftKey.isPressed)
                {
                    var selection = selectionService.SelectedObject;
                    if (!selection) { return true; }

                    var entity = selection.GetComponentFast<EntityComponent>();
                    if (!entity) { return true; }

                    var item = itemProvider.CreateFromEntity(entity);
                    service.Set(i, item);

                    return true;
                }
                else
                {
                    var item = items[i];
                    item?.Activate();

                    return true;
                }

            }
        }

        return false;
    }

}
