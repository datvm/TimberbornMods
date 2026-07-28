namespace TechTree.UI;

[BindSingleton]
public class TechTreeGraphService(TechTreeRegistry registry) : ILoadableSingleton
{

    public TechTreeGraph Graph { get; private set; } = null!;
    public FrozenDictionary<string, (TechTreeGraphCategory, TechTreeGraphNode)> NodesByTechId { get; private set; } = null!;

    public void Load()
    {
        Dictionary<string, (TechTreeGraphCategory, TechTreeGraphNode)> nodesByTechId = [];
        List<TechTreeGraphCategory> categories = [];

        foreach (var category in registry.Categories)
        {
            var nodes = LayoutCategory(category);
            var graphCategory = new TechTreeGraphCategory(category, nodes);
            categories.Add(graphCategory);

            foreach (var node in nodes)
            {
                nodesByTechId[node.TechItem.Id] = (graphCategory, node);
            }
        }

        Graph = new([.. categories]);
        NodesByTechId = nodesByTechId.ToFrozenDictionary();
    }

    const int UncategorizedRows = 3;

    /// <summary>
    /// Places techs on a unit grid within a category.
    /// Uncategorized: fixed 3-row columns (no prerequisites allowed).
    /// Other categories: X = prereq depth, Y = packed row following parents.
    /// </summary>
    ImmutableArray<TechTreeGraphNode> LayoutCategory(TechCategory category)
    {
        var techs = category.Techs;
        if (techs.Length == 0)
        {
            return [];
        }

        if (category.Id == TechTreeRegistry.DefaultCategoryId)
        {
            return LayoutUncategorized(techs);
        }

        return LayoutPrerequisiteTree(techs);
    }

    /// <summary>
    /// Fills columns top-to-bottom, 3 rows high: (0,0), (0,1), (0,2), (1,0), ...
    /// Techs are already ordered by Spec.Order then Id from the registry.
    /// </summary>
    static ImmutableArray<TechTreeGraphNode> LayoutUncategorized(ImmutableArray<TechItem> techs)
    {
        List<TechTreeGraphNode> nodes = new(techs.Length);

        for (int i = 0; i < techs.Length; i++)
        {
            int x = i / UncategorizedRows;
            int y = i % UncategorizedRows;
            nodes.Add(new(techs[i], x, y));
        }

        return [.. nodes];
    }

    ImmutableArray<TechTreeGraphNode> LayoutPrerequisiteTree(ImmutableArray<TechItem> techs)
    {
        Dictionary<string, TechItem> techsById = techs.ToDictionary(t => t.Id);
        Dictionary<string, List<TechItem>> sameCategoryPrereqs = [];

        foreach (var tech in techs)
        {
            List<TechItem> prereqs = [];
            foreach (var prereqId in tech.Spec.Prerequisites)
            {
                if (techsById.TryGetValue(prereqId, out var prereq))
                {
                    prereqs.Add(prereq);
                }
            }
            sameCategoryPrereqs[tech.Id] = prereqs;
        }

        Dictionary<string, int> depths = [];
        foreach (var tech in techs)
        {
            ComputeDepth(tech, sameCategoryPrereqs, depths);
        }

        Dictionary<string, int> yByTechId = AssignRows(techs, sameCategoryPrereqs, depths);

        return [.. techs
            .OrderBy(t => depths[t.Id])
            .ThenBy(t => yByTechId[t.Id])
            .Select(t => new TechTreeGraphNode(t, depths[t.Id], yByTechId[t.Id]))];
    }

    int ComputeDepth(
        TechItem tech,
        Dictionary<string, List<TechItem>> sameCategoryPrereqs,
        Dictionary<string, int> depths
    )
    {
        if (depths.TryGetValue(tech.Id, out var existing))
        {
            if (existing < 0)
            {
                throw new Exception($"Cycle detected in tech prerequisites involving: {tech.Id}");
            }
            return existing;
        }

        // Mark as visiting so cycles throw instead of stack overflowing.
        depths[tech.Id] = -1;

        var prereqs = sameCategoryPrereqs[tech.Id];
        int depth = 0;
        if (prereqs.Count > 0)
        {
            depth = 1;
            foreach (var prereq in prereqs)
            {
                depth = Math.Max(depth, ComputeDepth(prereq, sameCategoryPrereqs, depths) + 1);
            }
        }

        depths[tech.Id] = depth;
        return depth;
    }

    Dictionary<string, int> AssignRows(
        ImmutableArray<TechItem> techs,
        Dictionary<string, List<TechItem>> sameCategoryPrereqs,
        Dictionary<string, int> depths
    )
    {
        Dictionary<string, int> yByTechId = [];

        foreach (var layer in techs.GroupBy(t => depths[t.Id]).OrderBy(g => g.Key))
        {
            var ordered = layer
                .OrderBy(t => ParentBarycenter(t, sameCategoryPrereqs, yByTechId))
                .ThenBy(t => t.Spec.Order)
                .ThenBy(t => t.Id, StringComparer.Ordinal)
                .ToArray();

            for (int y = 0; y < ordered.Length; y++)
            {
                yByTechId[ordered[y].Id] = y;
            }
        }

        return yByTechId;
    }

    static float ParentBarycenter(
        TechItem tech,
        Dictionary<string, List<TechItem>> sameCategoryPrereqs,
        Dictionary<string, int> yByTechId
    )
    {
        var prereqs = sameCategoryPrereqs[tech.Id];
        if (prereqs.Count == 0)
        {
            // Roots have no parents; Spec.Order then breaks ties via ThenBy.
            return 0f;
        }

        float sum = 0f;
        foreach (var prereq in prereqs)
        {
            sum += yByTechId[prereq.Id];
        }
        return sum / prereqs.Count;
    }

}
