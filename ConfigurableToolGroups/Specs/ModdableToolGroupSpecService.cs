namespace ConfigurableToolGroups.Specs;

public class ModdableToolGroupSpecService(ISpecService specs) : ITemplateCollectionServiceTailRunner
{

    public ToolGroupDetails RootToolGroup { get; private set; }
    public FrozenDictionary<string, ToolGroupDetails> ToolGroupsByIds { get; private set; } = FrozenDictionary<string, ToolGroupDetails>.Empty;


    public void Run(TemplateCollectionService templateCollectionService)
    {
        var allToolGroups = Populate();
        AssignGroupParents(allToolGroups);
        DetectCircularParentage(allToolGroups);
        AssignToolParents(allToolGroups, templateCollectionService);
        Build(allToolGroups);
    }

    Dictionary<string, ToolGroupDetailsBuilder> Populate()
    {
        Dictionary<string, ToolGroupDetailsBuilder> allToolGroups = [];

        foreach (var item in specs.GetSpecs<BlockObjectToolGroupSpec>())
        {
            allToolGroups[item.Id] = new ToolGroupDetailsBuilder(item);
        }

        return allToolGroups;
    }

    void AssignGroupParents(Dictionary<string, ToolGroupDetailsBuilder> allToolGroups)
    {
        foreach (var toolGroup in allToolGroups.Values)
        {
            var parentSpec = toolGroup.parentSpec;
            if (parentSpec is null) { continue; }

            foreach (var parentId in parentSpec.ParentIds)
            {
                if (!allToolGroups.TryGetValue(parentId, out var parentGroup))
                {
                    Debug.LogWarning($"[{nameof(ConfigurableToolGroups)}] Tool group '{toolGroup.spec.Id}' specifies a parent group '{parentId}' that does not exist.");
                    continue;
                }

                parentGroup.childrenGroups.Add(toolGroup);
                toolGroup.parents.Add(parentGroup);
            }
        }
    }

    void DetectCircularParentage(Dictionary<string, ToolGroupDetailsBuilder> allToolGroups)
    {
        HashSet<string> visited = [];
        HashSet<string> stack = [];

        void Visit(ToolGroupDetailsBuilder group)
        {
            if (stack.Contains(group.spec.Id))
            {
                throw new InvalidDataException($"Recursive tool group parentage detected at group '{group.spec.Id}'.");
            }
            if (visited.Contains(group.spec.Id)) { return; }
            visited.Add(group.spec.Id);
            stack.Add(group.spec.Id);
            foreach (var parent in group.parents)
            {
                Visit(parent);
            }
            stack.Remove(group.spec.Id);
        }

        foreach (var group in allToolGroups.Values)
        {
            Visit(group);
        }
    }

    void AssignToolParents(Dictionary<string, ToolGroupDetailsBuilder> allToolGroups, TemplateCollectionService templateService)
    {
        foreach (var template in templateService.AllTemplates)
        {
            var tool = template.GetSpec<PlaceableBlockObjectSpec>();
            if (tool is null) { continue; }

            var templateName = tool.GetSpec<TemplateSpec>().TemplateName;

            var parentSpec = tool.GetSpec<ParentToolGroupSpec>();

            string[] parentIds = [tool.ToolGroupId, .. parentSpec?.ParentIds ?? []];
            foreach (var id in parentIds)
            {
                if (!allToolGroups.TryGetValue(id, out var parentGroup))
                {
                    Debug.LogWarning($"[{nameof(ConfigurableToolGroups)}] Tool '{templateName}' ('from {tool.Blueprint.Name}') specifies a parent group '{id}' that does not exist.");
                    continue;
                }

                parentGroup.childrenTools.Add(new(tool, templateName));
            }
        }
    }

    void Build(Dictionary<string, ToolGroupDetailsBuilder> allToolGroups)
    {
        Dictionary<ToolGroupDetailsBuilder, ToolGroupDetails> builtGroups = [];
        List<ToolGroupDetails> rootChildren = [];

        // Build children
        foreach (var grp in allToolGroups.Values)
        {
            builtGroups[grp] = new(grp.spec);
        }

        // Assign parents and children
        foreach (var grp in builtGroups.Keys)
        {
            var built = builtGroups[grp];

            if (grp.parents.Count == 0)
            {
                rootChildren.Add(built);
            }
            else
            {
                built.parents.AddRange(grp.parents
                    .Select(p => builtGroups[p])
                    .OrderBy(q => q.Spec.Order));
            }

            built.childrenGroups.AddRange(grp.childrenGroups
                .Select(cg => builtGroups[cg])
                .OrderBy(cg => cg.Spec.Order));

            built.childrenTools.AddRange(grp.childrenTools
                .OrderBy(t => t.Placeable.ToolOrder));
        }

        RootToolGroup = ToolGroupDetails.CreateRoot();
        RootToolGroup.childrenGroups.AddRange(rootChildren.OrderBy(g => g.Spec.Order));

        ToolGroupsByIds = builtGroups.Values.ToFrozenDictionary(g => g.Spec.Id);
    }

    class ToolGroupDetailsBuilder(BlockObjectToolGroupSpec spec)
    {
        public readonly BlockObjectToolGroupSpec spec = spec;
        public readonly List<ToolGroupDetailsBuilder> parents = [];
        public readonly List<ToolGroupDetailsBuilder> childrenGroups = [];
        public readonly List<PlaceableToolInfo> childrenTools = [];
        public readonly ParentToolGroupSpec? parentSpec = spec.GetSpec<ParentToolGroupSpec>();
    };

}

public readonly record struct ToolGroupDetails(BlockObjectToolGroupSpec Spec)
{
    internal readonly List<ToolGroupDetails> parents = [];
    internal readonly List<ToolGroupDetails> childrenGroups = [];
    internal readonly List<PlaceableToolInfo> childrenTools = [];

    public IReadOnlyList<ToolGroupDetails> Parents => parents;
    public IReadOnlyList<ToolGroupDetails> ChildrenGroups => childrenGroups;
    public IReadOnlyList<PlaceableToolInfo> ChildrenTools => childrenTools;

    public bool Empty => ChildrenGroups.Count == 0 && ChildrenTools.Count == 0;

    public static ToolGroupDetails CreateRoot() => new(new()
    {
        Id = "$SpecialRoot",
    });
}

public record PlaceableToolInfo(
    PlaceableBlockObjectSpec Placeable,
    string TemplateName
);