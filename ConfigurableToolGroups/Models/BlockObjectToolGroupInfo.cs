namespace ConfigurableToolGroups.Models;

public readonly record struct BlockObjectToolGroupInfo(BlockObjectToolGroupSpec Spec) : IToolButtonInfo
{
    internal readonly List<BlockObjectToolGroupInfo> parents = [];
    internal readonly List<BlockObjectToolGroupInfo> childrenGroups = [];
    internal readonly List<PlaceableToolInfo> childrenTools = [];
    internal readonly List<IToolButtonInfo> orderedChildren = [];

    public IReadOnlyList<BlockObjectToolGroupInfo> Parents => parents;
    public IReadOnlyList<BlockObjectToolGroupInfo> ChildrenGroups => childrenGroups;
    public IReadOnlyList<PlaceableToolInfo> ChildrenTools => childrenTools;
    public IReadOnlyList<IToolButtonInfo> OrderedChildren => orderedChildren;

    public bool Empty => ChildrenGroups.Count == 0 && ChildrenTools.Count == 0;

    public string Id => Spec.Id;
    public int DefaultOrder => Spec.Order;

    public static BlockObjectToolGroupInfo CreateRoot() => new(new()
    {
        Id = "$SpecialRoot",
    });
}
