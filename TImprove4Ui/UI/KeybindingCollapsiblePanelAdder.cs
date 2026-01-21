namespace TImprove4Ui.UI;

[BindSingleton(Contexts = BindAttributeContext.All)]
public class KeybindingCollapsiblePanelAdder(KeyBindingsBox keyBindingsBox) : IPostLoadableSingleton
{
    ImmutableArray<KeyValuePair<string, VisualElement>> namedRows = [];

    public void PostLoad()
    {
        var box = keyBindingsBox._root;
        var contentBox = keyBindingsBox._content;

        var txt = box.AddTextField("FilterBox", changeCallback: OnKeywordChanged);
        box.Insert(0, txt);

        List<KeyValuePair<string, VisualElement>> rows = [];

        foreach (var grp in contentBox.Children().ToArray())
        {
            var collapsible = new CollapsiblePanel();
            collapsible.SetTitle(grp.Q<Label>("Header").text);
            collapsible.SetExpand(false);

            collapsible.Container.Add(grp);
            keyBindingsBox._content.Add(collapsible);

            foreach (var row in grp.Query(className: "key-binding-row").Build())
            {
                var name = row.Q<Label>("Name").text;
                rows.Add(new(name.ToLower(), row));
            }
        }

        namedRows = [.. rows];
    }

    void OnKeywordChanged(string kw)
    {
        kw = kw.Trim().ToLower();
        var hasKeyword = kw.Length > 0;

        foreach (var (name, row) in namedRows)
        {
            row.SetDisplay(!hasKeyword || name.Contains(kw));
        }
    }

}
