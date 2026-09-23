namespace ScienceShop.UI;

static class RecipeDescriptionUi
{
    public static void AddInputs(VisualElement root, IEnumerable<VisualElement> items)
    {
        var container = root.Q<VisualElement>("Input");
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
            root.Q<VisualElement>("InputArrow")?.ToggleDisplayStyle(true);
        }
    }
}
