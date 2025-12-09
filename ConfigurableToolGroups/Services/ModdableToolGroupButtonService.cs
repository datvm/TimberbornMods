namespace ConfigurableToolGroups.Services;

public class ModdableToolGroupButtonService : IUnloadableSingleton
{

    static ModdableToolGroupButtonService? instance;
    public static ModdableToolGroupButtonService Instance => instance
        ?? throw new InvalidOperationException($"Service {nameof(ModdableToolGroupButtonService)} is not initialized.");

    readonly Dictionary<ToolGroupButton, ModdableToolGroupButtonInfo> buttons = [];
    public ModdableToolGroupButtonInfo? this[ToolGroupButton index] => buttons.TryGetValue(index, out var info) ? info : null;

    public ModdableToolGroupButtonService()
    {
        instance = this;
    }

    public ModdableToolGroupButtonInfo GetOrAddButton(ToolGroupButton button, ModdableToolGroupButtonInfo? parent)
    {
        if (!buttons.TryGetValue(button, out var info))
        {
            info = buttons[button] = new(button, parent);
        }

        parent?.Children.Add(button);

        return info;
    }

    public void Unload() => instance = null;


}
