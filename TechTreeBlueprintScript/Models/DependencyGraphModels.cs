namespace TechTreeBlueprintScript.Models;

public record BlueprintDependencyGraph(ImmutableArray<BlueprintDependencyNode> Roots)
{
    
    public void ScanNodes(Action<BlueprintDependencyNode> action)
    {
        HashSet<BlueprintDependencyNode> visited = [];
        foreach (var root in Roots)
        {
            ScanNode(root);
        }

        void ScanNode(BlueprintDependencyNode node)
        {
            if (!visited.Add(node))
            {
                return;
            }
            action(node);
            foreach (var child in node.Children)
            {
                ScanNode(child);
            }
        }
    }

    public async Task ScanNodesAsync(Func<BlueprintDependencyNode, Task> action)
    {
        HashSet<BlueprintDependencyNode> visited = [];
        foreach (var root in Roots)
        {
            await ScanNodeAsync(root);
        }

        async Task ScanNodeAsync(BlueprintDependencyNode node)
        {
            if (!visited.Add(node))
            {
                return;
            }
            await action(node);
            foreach (var child in node.Children)
            {
                await ScanNodeAsync(child);
            }
        }
    }

}

public record BlueprintDependencyNode(ParsedBuildingInfo Building)
{
    public List<BlueprintDependencyNode> Children { get; } = [];
    public List<BlueprintDependencyNode> Parents { get; } = [];
}

public record ParsedBuildingInfo(
    ScriptBlueprint Blueprint,
    int Order,
    string TemplateName,
    FrozenSet<string> Required,
    FrozenSet<string>? Produces = null,
    FrozenSet<string>? OtherTags = null,
    bool IsGatherer = false,
    bool IsManufactory = false,
    bool IsPlanter = false
)
{
    public FrozenSet<string> Produces { get; } = Produces ?? [];
    public FrozenSet<string> OtherTags { get; } = OtherTags ?? [];
}