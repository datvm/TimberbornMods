using UnityEngine.Assertions;

namespace ConfigurableToolGroups.Services;

public class ModdableToolGroupButtonService : ILoadableSingleton, IUnloadableSingleton
{

    static ModdableToolGroupButtonService? instance;
    public static ModdableToolGroupButtonService Instance => instance
        ?? throw new InvalidOperationException($"Service {nameof(ModdableToolGroupButtonService)} is not initialized.");

    readonly Dictionary<ToolGroupButton, ModdableToolGroupButtonInfo> buttons = [];
    readonly IAssetLoader assets;

    public ModdableToolGroupButtonInfo? this[ToolGroupButton index] => buttons.TryGetValue(index, out var info) ? info : null;

    public Texture2D BackgroundTexture { get; private set; } = null!;

    public ModdableToolGroupButtonService(IAssetLoader assets)
    {
        instance = this;
        this.assets = assets;
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

    public void Load()
    {
        BackgroundTexture = assets.Load<Texture2D>("UI/Images/BottomBar/bar-bg");
    }

    public void Unload() => instance = null;

}
