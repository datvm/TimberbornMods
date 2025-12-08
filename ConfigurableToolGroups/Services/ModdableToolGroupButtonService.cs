namespace ConfigurableToolGroups.Services;

public class ModdableToolGroupButtonService : IUnloadableSingleton
{

    static ModdableToolGroupButtonService? instance;
    public static ModdableToolGroupButtonService Instance => instance
        ?? throw new InvalidOperationException($"Service {nameof(ModdableToolGroupButtonService)} is not initialized.");

    readonly Dictionary<ToolGroupButton, ToolGroupButtonInfo> buttons = [];
    public ToolGroupButtonInfo? this[ToolGroupButton index] => buttons.TryGetValue(index, out var info) ? info : null;

    public ModdableToolGroupButtonService()
    {
        instance = this;
    }

    public ToolGroupButtonInfo AddButton(ToolGroupButton button, ToolGroupButtonInfo? parent) 
        => buttons[button] = new(button, parent);

    public void Unload() => instance = null;

    
}
