namespace TImprove4Ui.Services;

public class MaterialCounterExpansionService(
    TopBarPanel topBarPanel
) : IPostLoadableSingleton
{
    const string RootName = "Counter";

    public void PostLoad()
    {
        var shouldClose = MSettings.AutoExpandCounter;

        foreach (var counter in topBarPanel._counters)
        {
            if (counter is not ExtendableTopBarCounter extendableCounter) { continue; }

            var root = FindRootFor(extendableCounter);
            var items = root.Q("CounterItems");
            var toggler = root.Q<Button>("ExtensionToggler");
            var bg = root.Q("Background");

            var openByAuto = false;

            if (shouldClose)
            {
                ToggleVisibility(toggler, items, bg, false);
            }

            root.RegisterCallback<MouseEnterEvent>(_ =>
            {
                if (!MSettings.AutoExpandCounter) { return; }
                openByAuto = ToggleVisibility(toggler, items, bg, true);
            });

            root.RegisterCallback<MouseLeaveEvent>(_ =>
            {
                if (!MSettings.AutoExpandCounter || !openByAuto) { return; }
                ToggleVisibility(toggler, items, bg, false);
                openByAuto = false;
            });
        }
    }

    public static bool ToggleVisibility(Button toggler, VisualElement items, VisualElement background, bool visible)
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
