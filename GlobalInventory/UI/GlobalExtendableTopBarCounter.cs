namespace GlobalInventory.UI;

class GlobalExtendableTopBarCounter
{
    const string HiddenClass = "extension-clamp--hidden";

    readonly ImmutableArray<GlobalTopBarCounterRow> rows;
    readonly VisualElement root;
    readonly Label emptyPlaceholder;

    public GlobalExtendableTopBarCounter(
        ImmutableArray<GlobalTopBarCounterRow> rows,
        VisualElement root,
        Label emptyPlaceholder)
    {
        this.rows = rows;
        this.root = root;
        this.emptyPlaceholder = emptyPlaceholder;
    }

    public VisualElement Root => root;

    public void UpdateValues()
    {
        var anyVisible = false;
        foreach (var row in rows)
        {
            anyVisible |= row.UpdateVisible();
        }

        emptyPlaceholder.ToggleDisplayStyle(!anyVisible && rows.Length > 0);
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
