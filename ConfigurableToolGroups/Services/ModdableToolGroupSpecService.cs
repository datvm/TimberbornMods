namespace ConfigurableToolGroups.Services;

public class ModdableToolGroupSpecService(
    ISpecService specs,
    TemplateCollectionService templateCollectionService
) : ILoadableSingleton
{
    public BlockObjectToolGroupInfo RootToolGroup { get; private set; }
    public FrozenDictionary<string, BlockObjectToolGroupInfo> ToolGroupsByIds { get; private set; } = FrozenDictionary<string, BlockObjectToolGroupInfo>.Empty;

    public void Load()
    {
        var allToolGroups = Populate();
        AssignGroupParents(allToolGroups);
        DetectCircularParentage(allToolGroups);
        AssignToolParents(allToolGroups, templateCollectionService);
        Build(allToolGroups);
        Reorder();
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

        foreach (var toolGroup in allToolGroups.Values)
        {
            var childrenSpec = toolGroup.childrenSpec;
            if (childrenSpec is null) { continue; }

            foreach (var childGroupId in childrenSpec.ChildrenGroupsIds)
            {
                if (!allToolGroups.TryGetValue(childGroupId, out var childGroup))
                {
                    Debug.LogWarning($"[{nameof(ConfigurableToolGroups)}] Tool group '{toolGroup.spec.Id}' specifies a child group '{childGroupId}' that does not exist.");
                    continue;
                }

                if (!toolGroup.childrenGroups.Contains(childGroup))
                {
                    toolGroup.childrenGroups.Add(childGroup);
                }
                if (!childGroup.parents.Contains(toolGroup))
                {
                    childGroup.parents.Add(toolGroup);
                }
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
        var parentInfo = GatherToolParentsInfo(allToolGroups, templateService);

        foreach (var template in templateService.AllTemplates)
        {
            var tool = template.GetSpec<PlaceableBlockObjectSpec>();
            if (tool is null) { continue; }

            var templateName = tool.GetSpec<TemplateSpec>().TemplateName;

            IEnumerable<string> parentIds = parentInfo.TryGetValue(templateName, out var info) ? info.ParentGroups : [];

            var parentSpec = tool.GetSpec<ParentToolGroupSpec>();
            if (parentSpec is not null)
            {
                parentIds = parentIds.Concat(parentSpec.ParentIds);
            }

            if (info?.DoNotIncludePlaceableParent != true)
            {
                parentIds = parentIds.Append(tool.ToolGroupId);
            }

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

    Dictionary<string, ToolParentInfo> GatherToolParentsInfo(Dictionary<string, ToolGroupDetailsBuilder> allToolGroups, TemplateCollectionService templateService)
    {
        Dictionary<string, ToolParentInfo> result = [];

        foreach (var grp in allToolGroups.Values)
        {
            var childSpec = grp.childrenSpec;
            if (childSpec is null) { continue; }

            foreach (var toolTemplateName in childSpec.ChildrenToolsTemplateNames)
            {
                if (!result.TryGetValue(toolTemplateName, out var info))
                {
                    info = result[toolTemplateName] = new();
                }

                info.ParentGroups.Add(grp.spec.Id);
                if (childSpec.DoNotIncludePlaceableToolGroup)
                {
                    info.DoNotIncludePlaceableParent = true;
                }
            }
        }

        return result;
    }

    void Build(Dictionary<string, ToolGroupDetailsBuilder> allToolGroups)
    {
        Dictionary<ToolGroupDetailsBuilder, BlockObjectToolGroupInfo> builtGroups = [];
        List<BlockObjectToolGroupInfo> rootChildren = [];

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
                    .Select(p => builtGroups[p]));
            }

            built.childrenGroups.AddRange(grp.childrenGroups
                .Select(cg => builtGroups[cg]));

            built.childrenTools.AddRange(grp.childrenTools);
        }

        RootToolGroup = BlockObjectToolGroupInfo.CreateRoot();
        RootToolGroup.childrenGroups.AddRange(rootChildren);

        ToolGroupsByIds = builtGroups.Values.ToFrozenDictionary(g => g.Spec.Id);
    }

    void Reorder()
    {
        foreach (var group in ToolGroupsByIds.Values.Prepend(RootToolGroup))
        {
            Dictionary<string, int> orders = [];

            var childrenSpec = group.Spec.GetSpec<ToolGroupChildrenSpec>();
            if (childrenSpec is not null)
            {
                // Explicit order take precedent
                foreach (var (id, order) in childrenSpec.ChildrenExplicitOrderedIds)
                {
                    orders[id] = order;
                }

                // Then array/fluent order
                var curr = 0;
                foreach (var id in childrenSpec.ChildrenOrderedIds)
                {
                    if (!orders.ContainsKey(id))
                    {
                        orders[id] = ++curr * 10;
                    }
                }
            }

            // Finally if not found, order by their game assigned order
            group.orderedChildren.AddRange(group.childrenGroups
                .Cast<IToolButtonInfo>()
                .Concat(group.childrenTools)
                .OrderBy(q => orders.TryGetValue(q.Id, out var order) ? order : q.DefaultOrder));
        }
    }

    class ToolGroupDetailsBuilder(BlockObjectToolGroupSpec spec)
    {
        public readonly BlockObjectToolGroupSpec spec = spec;
        public readonly List<ToolGroupDetailsBuilder> parents = [];
        public readonly List<ToolGroupDetailsBuilder> childrenGroups = [];
        public readonly List<PlaceableToolInfo> childrenTools = [];
        public readonly ParentToolGroupSpec? parentSpec = spec.GetSpec<ParentToolGroupSpec>();
        public readonly ToolGroupChildrenSpec? childrenSpec = spec.GetSpec<ToolGroupChildrenSpec>();
    };

    class ToolParentInfo
    {
        public readonly HashSet<string> ParentGroups = [];
        public bool DoNotIncludePlaceableParent { get; set; }
    }

}
