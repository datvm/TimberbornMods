namespace ConfigurableToolGroups.Services;

public class ToolPanelPositioningService(ToolPanel toolPanel) : ILoadableSingleton, IUnloadableSingleton
{
    const int ButtonHeight = 54;

    public static ToolPanelPositioningService? Instance { get; private set; }

    VisualElement root = null!;

    public void SetTop(int level) => root.style.top = -ButtonHeight * (level + 1);

    public void Load()
    {
        root = toolPanel._uiLayout._bottomBar.Q(className: "tool-panel")
            ?? throw new InvalidOperationException("Could not find tool panel root element.");
        Instance = this;

        root.style.position = Position.Relative;
    }

    public void Unload()
    {
        Instance = null;
    }
}
