namespace ConfigurableToolGroups.Models;

public readonly record struct ToolGroupInfo(BlockObjectToolGroupSpec Spec) : IToolButtonInfo
{
    internal readonly List<ToolGroupInfo> parents = [];
    internal readonly List<ToolGroupInfo> childrenGroups = [];
    internal readonly List<PlaceableToolInfo> childrenTools = [];
    internal readonly List<IToolButtonInfo> orderedChildren = [];

    public IReadOnlyList<ToolGroupInfo> Parents => parents;
    public IReadOnlyList<ToolGroupInfo> ChildrenGroups => childrenGroups;
    public IReadOnlyList<PlaceableToolInfo> ChildrenTools => childrenTools;
    public IReadOnlyList<IToolButtonInfo> OrderedChildren => orderedChildren;

    public bool Empty => ChildrenGroups.Count == 0 && ChildrenTools.Count == 0;

    public string Id => Spec.Id;

    public static ToolGroupInfo CreateRoot() => new(new()
    {
        Id = "$SpecialRoot",
    });
}
