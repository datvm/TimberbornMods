namespace TImprove4Ui.UI;

[BindSingleton(Contexts = BindAttributeContext.All)]
public class KeybindingCollapsiblePanelAdder(KeyBindingsBox keyBindingsBox, ILoc t) : IPostLoadableSingleton
{
    readonly record struct NamedRow(string Name, VisualElement Row);
    readonly record struct NamedGroup(CollapsiblePanel Panel, ImmutableArray<NamedRow> Rows);

    ImmutableArray<NamedGroup> namedGroups = [];

    public void PostLoad()
    {
        var box = keyBindingsBox._root;
        var contentBox = keyBindingsBox._content;

        var filterRow = box.AddRow().AlignItems().SetMarginBottom();
        box.Insert(0, filterRow);

        var txt = filterRow.AddTextField("FilterBox", changeCallback: OnKeywordChanged).SetFlexGrow().SetMarginRight();
        filterRow.AddGameButtonPadded(t.T("LV.T4UI.CollapseAll"), () => ToggleExpandAll(false), paddingY: 3).SetMarginRight(5);
        filterRow.AddGameButtonPadded(t.T("LV.T4UI.ExpandAll"), () => ToggleExpandAll(true), paddingY: 3);

        List<NamedGroup> groups = [];

        foreach (var grp in contentBox.Children().ToArray())
        {
            var collapsible = new CollapsiblePanel();
            collapsible.SetTitle(grp.Q<Label>("Header").text);
            collapsible.SetExpand(false);

            collapsible.Container.Add(grp);
            keyBindingsBox._content.Add(collapsible);

            List<NamedRow> rows = [];
            foreach (var row in grp.Query(className: "key-binding-row").Build())
            {
                var name = row.Q<Label>("Name").text;
                rows.Add(new(name.ToLower(), row));
            }

            groups.Add(new(collapsible, [.. rows]));
        }

        namedGroups = [.. groups];
    }

    void ToggleExpandAll(bool expand)
    {
        foreach (var group in namedGroups)
        {
            group.Panel.SetExpand(expand);
        }
    }

    void OnKeywordChanged(string kw)
    {
        kw = kw.Trim().ToLower();
        var hasKeyword = kw.Length > 0;

        foreach (var group in namedGroups)
        {
            var visibleCount = 0;

            foreach (var row in group.Rows)
            {
                var visible = !hasKeyword || row.Name.Contains(kw);
                row.Row.SetDisplay(visible);
                if (visible)
                {
                    visibleCount++;
                }
            }

            group.Panel.SetDisplay(!hasKeyword || visibleCount > 0);

            if (hasKeyword && visibleCount > 0)
            {
                group.Panel.SetExpand(true);
            }
        }
    }

}
