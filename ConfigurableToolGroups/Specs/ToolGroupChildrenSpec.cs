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
    public bool DoNotIncludePlaceableToolGroup { get; init; }

}
