namespace GlobalInventory.UI;

static class RecipeDescriptionUi
{
    public static void AddInputs(VisualElement root, IEnumerable<VisualElement> items)
        => Add(root, "Input", "InputArrow", items);

    public static void AddOutputs(VisualElement root, IEnumerable<VisualElement> items)
        => Add(root, "Output", "OutputArrow", items);

    static void Add(VisualElement root, string itemsName, string arrowName, IEnumerable<VisualElement> items)
    {
        var container = root.Q<VisualElement>(itemsName);
        if (container is null)
        {
            return;
        }

        var added = false;
        foreach (var item in items)
        {
            container.Add(item);
            added = true;
        }

        if (added)
        {
            root.Q<VisualElement>(arrowName)?.ToggleDisplayStyle(true);
        }
    }
}
