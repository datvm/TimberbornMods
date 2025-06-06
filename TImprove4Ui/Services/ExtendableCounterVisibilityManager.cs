namespace TImprove4Ui.Services;

public class ExtendableCounterVisibilityManager
{
    const string RootName = "Counter";

    readonly Func<bool, bool> setVisibility;

    bool openByAuto;
    bool openByTool;

    public ExtendableCounterVisibilityManager(ExtendableTopBarCounter counter)
    {
        var root = FindRootFor(counter);

        var items = root.Q("CounterItems");
        var toggler = root.Q<Button>("ExtensionToggler");
        var bg = root.Q("Background");

        setVisibility = visibile => ToggleVisibility(toggler, items, bg, visibile);
        RegisterHoverCallbacks(root);
    }

    void RegisterHoverCallbacks(VisualElement root)
    {
        root.RegisterCallback<MouseEnterEvent>(_ =>
        {
            if (!MSettings.AutoExpandCounter) { return; }
            openByAuto = ToggleVisibility(true);
        });

        root.RegisterCallback<MouseLeaveEvent>(_ =>
        {
            if (!MSettings.AutoExpandCounter || !openByAuto) { return; }
            ToggleVisibility(false);
            openByAuto = false;
        });

        root.RegisterCallback<ClickEvent>(_ =>
        {
            openByAuto = false;
        });
    }

    public bool ToggleVisibility(bool visible) => setVisibility(visible);

    public void OpenByTool()
    {
        openByTool = ToggleVisibility(true);
    }

    public void CloseByTool()
    {
        if (!openByTool) { return; }
        ToggleVisibility(false);
        openByTool = false;
    }

    static bool ToggleVisibility(Button toggler, VisualElement items, VisualElement background, bool visible)
    {
        var isVisible = items.IsDisplayed();
        if (isVisible == visible) { return false; }

        TopBarCounterFactory.ToggleVisibility(toggler, items, background);
        return true;
    }

    static VisualElement FindRootFor(ExtendableTopBarCounter counter)
    {
        VisualElement root = counter._value;

        while (root.name != RootName)
        {
            root = root.parent;

            if (root is null)
            {
                throw new InvalidOperationException($"Could not find {RootName} for {counter}. The game UI was probably updated.");
            }
        }

        return root;
    }

}