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

    /// <summary>
    /// Layout rules (unit grid, not pixels):
    /// <list type="number">
    /// <item>Y authored; Y &lt; 0 ⇒ overflow first row.</item>
    /// <item>Condense each row by Order; <see cref="TechTreeItemSpec.LeftX"/> is applied here only
    /// (empty columns before the node) so parents are not shifted after children read them.</item>
    /// <item>Iterate: push right of prerequisites, then fix same-row X collisions (no further LeftX).</item>
    /// </list>
    /// Spec.X ≥ 0 forces an initial column before the iterative passes.
    /// </summary>
    static ImmutableArray<TechTreeGraphNode> LayoutCategory(TechCategory category)
    {
        var techs = category.Techs;
        if (techs.Length == 0)
        {
            return [];
        }

        List<TechItem> unplaced = [];
        List<TechItem> placed = [];

        foreach (var tech in techs)
        {
            if (tech.Spec.HasAuthoredRow)
            {
                placed.Add(tech);
            }
            else
            {
                unplaced.Add(tech);
            }
        }

        int yOffset = unplaced.Count > 0 ? 1 : 0;
        List<TechTreeGraphNode> nodes = new(techs.Length);

        if (unplaced.Count > 0)
        {
            var orderedUnplaced = unplaced
                .OrderBy(t => t.Spec.Order)
                .ThenBy(t => t.Id, StringComparer.Ordinal)
                .ToArray();

            int prevX = -1;
            foreach (var tech in orderedUnplaced)
            {
                int x = NextXWithLeftPad(prevX, tech.Spec.LeftX, forced: null);
                nodes.Add(new(tech, x, Y: 0));
                prevX = x;
            }
        }

        if (placed.Count == 0)
        {
            return [.. nodes];
        }

        Dictionary<string, TechItem> placedById = placed.ToDictionary(t => t.Id);
        Dictionary<string, int> depths = [];
        foreach (var tech in placed)
        {
            ComputeDepth(tech, placedById, depths);
        }

        // 1) Condense per row by Order — LeftX applied once, up front.
        Dictionary<string, int> xById = [];
        Dictionary<string, int> yById = [];

        foreach (var rowGroup in placed.GroupBy(t => t.Spec.Y).OrderBy(g => g.Key))
        {
            int y = rowGroup.Key + yOffset;
            var row = rowGroup
                .OrderBy(t => t.Spec.Order)
                .ThenBy(t => t.Id, StringComparer.Ordinal)
                .ToList();

            int prevX = -1;
            foreach (var tech in row)
            {
                int? forced = tech.Spec.HasForcedColumn ? tech.Spec.X : null;
                int x = NextXWithLeftPad(prevX, tech.Spec.LeftX, forced);
                xById[tech.Id] = x;
                yById[tech.Id] = y;
                prevX = x;
            }
        }

        // 2) Prereq push + collision fix until stable.
        //    LeftX is not reapplied — only "X > parent" and unique cells per row.
        const int maxPasses = 32;
        for (int pass = 0; pass < maxPasses; pass++)
        {
            bool changed = false;

            foreach (var tech in placed
                .OrderBy(t => depths[t.Id])
                .ThenBy(t => t.Spec.Order)
                .ThenBy(t => t.Id, StringComparer.Ordinal))
            {
                int minX = 0;
                foreach (var prereqId in tech.Spec.Prerequisites)
                {
                    if (xById.TryGetValue(prereqId, out var parentX))
                    {
                        minX = Math.Max(minX, parentX + 1);
                    }
                }

                if (xById[tech.Id] < minX)
                {
                    xById[tech.Id] = minX;
                    changed = true;
                }
            }

            foreach (var rowGroup in placed.GroupBy(t => yById[t.Id]).OrderBy(g => g.Key))
            {
                var row = rowGroup
                    .OrderBy(t => xById[t.Id])
                    .ThenBy(t => t.Spec.Order)
                    .ThenBy(t => t.Id, StringComparer.Ordinal)
                    .ToList();

                HashSet<int> usedX = [];
                foreach (var tech in row)
                {
                    int x = xById[tech.Id];
                    while (usedX.Contains(x))
                    {
                        x++;
                        changed = true;
                    }

                    if (xById[tech.Id] != x)
                    {
                        xById[tech.Id] = x;
                        changed = true;
                    }

                    usedX.Add(x);
                }
            }

            if (!changed)
            {
                break;
            }
        }

        foreach (var tech in placed)
        {
            nodes.Add(new(tech, xById[tech.Id], yById[tech.Id]));
        }

        return [.. nodes
            .OrderBy(n => n.Y)
            .ThenBy(n => n.X)
            .ThenBy(n => n.TechItem.Id, StringComparer.Ordinal)];
    }

    /// <summary>
    /// Next column on a row: after <paramref name="prevX"/>, skip <paramref name="leftX"/> empties,
    /// or at least <paramref name="forced"/> when set.
    /// </summary>
    static int NextXWithLeftPad(int prevX, int leftX, int? forced)
    {
        int pad = Math.Max(0, leftX);
        int minFromPrev = prevX < 0 ? pad : prevX + 1 + pad;

        if (forced is { } f)
        {
            return Math.Max(f, minFromPrev);
        }

        return minFromPrev;
    }

    static int ComputeDepth(
        TechItem tech,
        Dictionary<string, TechItem> placedById,
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

        depths[tech.Id] = -1;

        int depth = 0;
        foreach (var prereqId in tech.Spec.Prerequisites)
        {
            if (!placedById.TryGetValue(prereqId, out var prereq))
            {
                continue;
            }

            depth = Math.Max(depth, ComputeDepth(prereq, placedById, depths) + 1);
        }

        depths[tech.Id] = depth;
        return depth;
    }

}
