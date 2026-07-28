namespace TechTree.Models;

public record TechTreeGraph(ImmutableArray<TechTreeGraphCategory> Categories);
public record TechTreeGraphCategory(TechCategory TechCategory, ImmutableArray<TechTreeGraphNode> Nodes);
public record TechTreeGraphNode(TechItem TechItem, int X, int Y);