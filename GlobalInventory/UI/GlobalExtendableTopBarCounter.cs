namespace GlobalInventory.UI;

class GlobalExtendableTopBarCounter(
    ImmutableArray<GlobalTopBarCounterRow> rows,
    VisualElement root,
    Label emptyPlaceholder)
{
    const string HiddenClass = "extension-clamp--hidden";

    public void UpdateValues()
    {
        var anyVisible = false;
        foreach (var row in rows)
        {
            row.Update(out var isVisible);
            anyVisible |= isVisible;
        }

        emptyPlaceholder.ToggleDisplayStyle(!anyVisible);
        root.ToggleDisplayStyle(anyVisible);
    }

    public static void ConfigureToggling(VisualElement root, VisualElement items)
    {
        var toggler = root.Q<Button>("ExtensionToggler");
        var background = root.Q<VisualElement>("Background");
        root.Q<VisualElement>("CounterWrapper").RegisterCallback<ClickEvent>(_ => Toggle(toggler, items, background));
        toggler.RegisterCallback<ClickEvent>(_ => Toggle(toggler, items, background));
    }

    static void Toggle(Button toggler, VisualElement items, VisualElement background)
    {
        var displayed = items.IsDisplayed();
        toggler.EnableInClassList(HiddenClass, displayed);
        background.ToggleDisplayStyle(!displayed);
        items.ToggleDisplayStyle(!displayed);
    }
}
