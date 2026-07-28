namespace TechTree.UI;

[BindSingleton]
public class TechTreePanel(
    TechTreeGraphService graphService,
    VisualElementInitializer veInit,
    IContainer container
) : VisualElement, ILoadableSingleton
{

#nullable disable
    ScrollView content;
#nullable enable

    public ImmutableArray<TechTreeCategoryElement> Categories;
    public FrozenDictionary<string, (TechTreeCategoryElement, TechTreeItemElement)> NodesByTechId { get; private set; } = null!;

    public void Load()
    {
        BuildUI();
        RenderGraph();
    }

    void BuildUI()
    {
        this.AlignItems(Align.Stretch);
        content = this.AddScrollView().SetFlexGrow().Initialize(veInit);
        content.horizontalScrollerVisibility = content.verticalScrollerVisibility = ScrollerVisibility.Auto;
    }

    void RenderGraph()
    {
        content.Clear();

        var graph = graphService.Graph;

        List<TechTreeCategoryElement> categories = [];
        Dictionary<string, (TechTreeCategoryElement, TechTreeItemElement)> nodesByTechId = [];

        foreach (var c in graph.Categories)
        {
            if (c.Nodes.Length == 0)
            {
                continue;
            }

            var categoryEl = container.GetInstance<TechTreeCategoryElement>();
            categoryEl.SetCategory(c);
            content.Add(categoryEl);
            categories.Add(categoryEl);

            foreach (var nodeEl in categoryEl.Nodes)
            {
                nodesByTechId[nodeEl.Node.TechItem.Id] = (categoryEl, nodeEl);
            }
        }

        Categories = [.. categories];
        NodesByTechId = nodesByTechId.ToFrozenDictionary();
    }

}
