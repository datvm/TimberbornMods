namespace TimberLive.Services;

/// <summary>
/// Converts HTTP automation data into client-optimized models for UI rendering and Mermaid visualization.
/// </summary>
[SelfService(Lifetime = ServiceLifetime.Singleton)]
public partial class AutomationsService
{
    /// <summary>
    /// Converts HttpAutomationMap into partitioned ClientAutomationMap with connected components.
    /// </summary>
    /// <remarks>
    /// Uses Union-Find to identify connected components. Partitions are sorted by size (largest first).
    /// Isolated automators (single nodes with no connections) are marked as IsIsolated for special UI handling.
    /// </remarks>
    public ClientAutomationMap ConvertToClientMap(HttpAutomationMap httpMap)
    {
        var automatorCount = httpMap.Automators.Count;

        // Union-Find for connected components
        var uf = new UnionFind(automatorCount);
        var idToIndex = new Dictionary<Guid, int>(automatorCount);

        for (int i = 0; i < automatorCount; i++)
        {
            var automator = httpMap.Automators[i];
            idToIndex[automator.Entity.EntityId] = i;
        }

        // Union automators connected by edges
        for (int i = 0; i < automatorCount; i++)
        {
            var automator = httpMap.Automators[i];
            var fromIndex = idToIndex[automator.Entity.EntityId];

            foreach (var input in automator.Inputs)
            {
                if (!input.FromAutomatorId.HasValue)
                {
                    continue;
                }

                if (idToIndex.TryGetValue(input.FromAutomatorId.Value, out var toIndex))
                {
                    uf.Union(fromIndex, toIndex);
                }
            }
        }

        // Group automators by partition root
        var groupsByRoot = new Dictionary<int, List<HttpAutomator>>();
        for (int i = 0; i < automatorCount; i++)
        {
            var root = uf.Find(i);
            if (!groupsByRoot.TryGetValue(root, out var group))
            {
                group = [];
                groupsByRoot[root] = group;
            }

            group.Add(httpMap.Automators[i]);
        }

        // Largest partitions first
        var groupEntries = groupsByRoot.ToList();
        groupEntries.Sort((left, right) => right.Value.Count.CompareTo(left.Value.Count));

        var result = new ClientAutomationMap();

        for (int i = 0; i < groupEntries.Count; i++)
        {
            var automators = groupEntries[i].Value;
            var automatorIds = new HashSet<Guid>();

            foreach (var automator in automators)
            {
                automatorIds.Add(automator.Entity.EntityId);
            }

            var partition = new ClientAutomationPartition { Id = i };
            var nodesById = new Dictionary<Guid, ClientAutomatorNode>(automators.Count);

            foreach (var automator in automators)
            {
                var node = new ClientAutomatorNode
                {
                    Id = automator.Entity.EntityId,
                    Kind = automator.Kind,
                    State = automator.State,
                    IsCyclicOrBlocked = automator.IsCyclicOrBlocked,
                    Label = GetLabel(automator, httpMap.Buildings)
                };

                partition.Automators.Add(node);
                nodesById[node.Id] = node;
                result.PartitionByAutomatorId[node.Id] = partition;
            }

            foreach (var automator in automators)
            {
                var toId = automator.Entity.EntityId;

                foreach (var input in automator.Inputs)
                {
                    if (!input.FromAutomatorId.HasValue)
                    {
                        continue;
                    }

                    var fromId = input.FromAutomatorId.Value;
                    if (!automatorIds.Contains(fromId))
                    {
                        continue;
                    }

                    partition.Edges.Add(new ClientAutomatorEdge
                    {
                        From = nodesById[fromId],
                        To = nodesById[toId],
                        State = input.State
                    });
                }
            }

            result.Partitions.Add(partition);
        }

        foreach (var kvp in httpMap.Buildings.Buildings)
        {
            result.Buildings[kvp.Key] = kvp.Value;
        }

        return result;
    }

    static string GetLabel(HttpAutomator automator, HttpBuildingsResult buildings)
    {
        return buildings.Buildings.TryGetValue(automator.Entity.EntityId, out var building)
            ? !string.IsNullOrEmpty(building.LabelName) ? building.LabelName : building.Name.EntityName
            : automator.Entity.EntityId.ToString();
    }
}
