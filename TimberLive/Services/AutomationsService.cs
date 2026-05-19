namespace TimberLive.Services;

/// <summary>
/// Converts HTTP automation data into client-optimized models for UI rendering and Mermaid visualization.
/// </summary>
[SelfService(Lifetime = ServiceLifetime.Singleton)]
public partial class AutomationsService(AutomationBuildingRegistry automationBuildingRegistry)
{
    static readonly Dictionary<Type, (Type, MethodInfo?)> ClientAutomatorNodeTypeCache = [];

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
                var building = httpMap.Buildings.Buildings[automator.Entity.EntityId];
                var node = CreateNode(automator, building);

                partition.Automators.Add(node);
                nodesById[node.Id] = node;
                result.PartitionByAutomatorId[node.Id] = partition;
            }

            foreach (var automator in automators)
            {
                var toId = automator.Entity.EntityId;

                for (var inputIndex = 0; inputIndex < automator.Inputs.Count; inputIndex++)
                {
                    var input = automator.Inputs[inputIndex];
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
                        State = input.State,
                        InputIndex = inputIndex,
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

    ClientAutomatorNode CreateNode(HttpAutomator automator, HttpBuilding building)
    {
        IClientAutomationBuilding? automationBuilding = null;
        object? automationSettings = null;
        if (automationBuildingRegistry.FindAutomationBuilding(building, out var found))
        {
            automationBuilding = found.Value.Item1;
            automationSettings = found.Value.Item2;
        }

        var (nodeType, propertySetter) = automationBuilding is null
            ? (typeof(ClientAutomatorNode), null)
            : ClientAutomatorNodeTypeCache.GetOrAdd(automationBuilding.SettingsType, static t =>
            {
                var resolvedNodeType = typeof(ClientAutomatorNode<>).MakeGenericType(t);
                return (resolvedNodeType, resolvedNodeType.GetProperty(nameof(ClientAutomatorNode<>.Settings))?.SetMethod);
            });

        var node = (ClientAutomatorNode)Activator.CreateInstance(nodeType)!;

        node.Id = automator.Entity.EntityId;
        node.Kind = automator.Kind;

        if (automator.AutomatableState is not null)
        {
            node.State = automator.AutomatableState == HttpAutomationConnectionState.On ? HttpAutomatorState.On : HttpAutomatorState.Off;
        }
        else
        {
            node.State = automator.AutomatorState;
        }

        node.IsCyclicOrBlocked = automator.IsCyclicOrBlocked;
        node.Label = building.Name.EntityName;
        node.AutomationBuilding = automationBuilding;
        node.AutomationSettings = automationSettings;

        propertySetter?.Invoke(node, [automationSettings]);

        return node;
    }

}
