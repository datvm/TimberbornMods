namespace TechTree.UI;

[BindTransient]
public class TechTreeCategoryElement(IContainer container) : VisualElement
{

#nullable disable
    public TechTreeGraphCategory Category { get; private set; }
    VisualElement canvas;
    TechTreeEdgeLayer edgeLayer;
#nullable enable
    public ImmutableArray<TechTreeItemElement> Nodes { get; private set; }

    public void SetCategory(TechTreeGraphCategory category)
    {
        Category = category;

        BuildUI();
        BuildItems();
    }

    void BuildUI()
    {
        Clear();
        this.SetMarginBottom(10).AlignItems(Align.Stretch);

        var collapsible = this.AddCollapsiblePanel(Category.TechCategory.Name);
        collapsible.SetMarginBottom(0);

        if (Category.TechCategory.Spec.Icon?.Asset is { } icon)
        {
            var header = collapsible.HeaderLabel.parent;
            var img = new Image { sprite = icon }
                .SetSize(24, 24)
                .SetMarginRight(6)
                .SetFlexShrink(0);
            header.Insert(0, img);
        }

        canvas = collapsible.Container.AddChild()
            .SetMarginBottom(0);
        canvas.style.position = Position.Relative;
        canvas.style.overflow = Overflow.Visible;

        edgeLayer = new TechTreeEdgeLayer();
        canvas.Add(edgeLayer);
    }

    void BuildItems()
    {
        List<TechTreeItemElement> nodes = [];
        Dictionary<string, TechTreeItemElement> byId = [];
        int maxX = 0;
        int maxY = 0;

        foreach (var item in Category.Nodes)
        {
            var itemEl = container.GetInstance<TechTreeItemElement>();
            itemEl.SetItem(item);
            canvas.Add(itemEl);

            maxX = Math.Max(maxX, item.X);
            maxY = Math.Max(maxY, item.Y);
            nodes.Add(itemEl);
            byId[item.TechItem.Id] = itemEl;
        }

        Nodes = [.. nodes];

        if (Category.Nodes.Length == 0)
        {
            canvas.SetSize(0, 0);
            edgeLayer.SetEdges([]);
            return;
        }

        float width = (maxX + 1) * (TechTreeItemElement.ItemWidth + TechTreeItemElement.GapX)
            - TechTreeItemElement.GapX;
        float height = (maxY + 1) * (TechTreeItemElement.ItemHeight + TechTreeItemElement.GapY)
            - TechTreeItemElement.GapY;

        canvas.SetSize(width, height);
        edgeLayer.SetSize(width, height);
        edgeLayer.SetEdges(BuildEdges(byId));
    }

    static List<(Vector2 From, Vector2 To)> BuildEdges(Dictionary<string, TechTreeItemElement> byId)
    {
        List<(Vector2 From, Vector2 To)> edges = [];

        foreach (var toEl in byId.Values)
        {
            foreach (var prereqId in toEl.Node.TechItem.Spec.Prerequisites)
            {
                if (!byId.TryGetValue(prereqId, out var fromEl))
                {
                    continue;
                }

                edges.Add((fromEl.CanvasRightAnchor, toEl.CanvasLeftAnchor));
            }
        }

        return edges;
    }

}
