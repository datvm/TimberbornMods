namespace ConfigurableToolGroups.Specs;

public record ToolGroupChildrenSpec : ComponentSpec
{

    [Serialize]
    public ImmutableArray<string> ChildrenGroupsIds { get; init; } = [];

    [Serialize]
    public ImmutableArray<string> ChildrenToolsTemplateNames { get; init; } = [];

    [Serialize]
    public ImmutableArray<string> ChildrenOrderedIds { get; init; } = [];

    [Serialize]
    public ImmutableArray<OrderedIds> ChildrenExplicitOrderedIds { get; init; } = [];

    [Serialize]
    public bool DoNotIncludePlaceableToolGroup { get; init; }

}

public record OrderedIds
{

    [Serialize]
    public string Id { get; init; } = null!;

    [Serialize]
    public int Order { get; init; }

    public void Deconstruct(out string id, out int order)
    {
        id = Id;
        order = Order;
    }

}