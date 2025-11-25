namespace ConfigurableToolGroups.Services;

public class ModdableToolGroupButtonService : IUnloadableSingleton
{

    static ModdableToolGroupButtonService? instance;
    public static ModdableToolGroupButtonService Instance => instance
        ?? throw new InvalidOperationException($"Service {nameof(ModdableToolGroupButtonService)} is not initialized.");

    readonly Dictionary<ToolGroupButton, ToolGroupButtonInfo> buttons = [];
    readonly ILoc t;

    public ToolGroupButtonInfo? this[ToolGroupButton index] => buttons.TryGetValue(index, out var info) ? info : null;

    public ModdableToolGroupButtonService(ILoc t)
    {
        instance = this;
        this.t = t;
    }

    public void Unload() => instance = null;

    public ToolGroupButtonInfo AddButton(ToolGroupButton button, ToolGroupButtonInfo? parent)
    {
        button.Root.Q<Label>("Tooltip").text = t.T(button._toolGroup.DisplayNameLocKey);

        return buttons[button] = new(button, parent);
    }

    
}
