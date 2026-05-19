namespace TimberLive.Services;

[SelfService(Lifetime = ServiceLifetime.Singleton)]
public class MermaidChartService
{
    public string ConvertToMermaid(ClientAutomationMap map)
    {
        if (map.Partitions.Count == 0)
        {
            return "flowchart LR\n";
        }

        StringBuilder builder = new();
        builder.AppendLine("flowchart LR");

        var partitions = map.Partitions;
        for (var i = partitions.Count - 1; i >= 0; i--)
        {
            var partition = partitions[i];
            builder.AppendLine($"  subgraph partition_{partition.Id}[\"Partition {partition.Id} ({partition.Automators.Count})\"]");
            builder.AppendLine("    direction LR");

            var partitionBody = ConvertPartitionToMermaidLines(partition, "    ");
            if (!string.IsNullOrEmpty(partitionBody))
            {
                builder.Append(partitionBody);
            }

            builder.AppendLine("  end");
            builder.AppendLine($"  class partition_{partition.Id} partition;");

            if (i < partitions.Count - 1)
            {
                builder.AppendLine();
            }
        }

        return builder.ToString();
    }

    public string ConvertToMermaid(ClientAutomationPartition partition)
    {
        StringBuilder builder = new();
        builder.AppendLine("flowchart LR");
        builder.AppendLine("  direction LR");
        builder.Append(ConvertPartitionToMermaidLines(partition, "  "));
        return builder.ToString();
    }

    string ConvertPartitionToMermaidLines(ClientAutomationPartition partition, string indent)
    {
        StringBuilder builder = new();

        foreach (var node in partition.Automators)
        {
            var nodeId = GetMermaidEntityId(node.Id);
            var nameLabel = EscapeMermaidText(node.Label);
            var typeLabel = EscapeMermaidText(GetAutomatorTypeLabel(node));
            var valueLabel = GetAutomatorValueLabel(node);
            var nodeLabel = string.IsNullOrEmpty(valueLabel)
                ? $"{nameLabel}<br/>{typeLabel}"
                : $"{nameLabel}<br/>{typeLabel}<br/>{EscapeMermaidText(valueLabel)}";

            builder.AppendLine($"{indent}{nodeId}[\"{nodeLabel}\"]");
            builder.AppendLine($"{indent}class {nodeId} {GetStateClass(node.State)};");
        }

        var consumedEdgeKeys = new HashSet<string>();
        var relayEdgeGroups = partition.Edges
            .Where(e => e.To.AutomationSettings is RelaySettingsModel)
            .GroupBy(e => e.To.Id);

        foreach (var relayEdgeGroup in relayEdgeGroups)
        {
            var edges = relayEdgeGroup.ToList();
            if (edges.Count < 1)
            {
                continue;
            }

            var relayNode = edges[0].To;
            var relayOperator = GetRelayOperatorLabel(relayNode);
            if (string.IsNullOrEmpty(relayOperator))
            {
                continue;
            }

            var relayId = GetMermaidEntityId(relayNode.Id);
            var mergeId = GetMermaidOperatorId(relayNode.Id);
            builder.AppendLine($"{indent}{mergeId}{{\"{EscapeMermaidText(relayOperator)}\"}}");
            builder.AppendLine($"{indent}class {mergeId} {GetStateClass(relayNode.State)};");

            foreach (var edge in edges)
            {
                var fromId = GetMermaidEntityId(edge.From.Id);
                var slotLabel = GetSlotLabel(edge.To, edge.InputIndex);
                var edgeState = ToConnectionState(edge.From.State);

                if (!string.IsNullOrEmpty(slotLabel))
                {
                    AppendEdge(builder, indent, fromId, mergeId, EscapeMermaidText(slotLabel), edgeState);
                }
                else
                {
                    AppendEdge(builder, indent, fromId, mergeId, null, edgeState);
                }

                consumedEdgeKeys.Add(GetEdgeKey(edge));
            }

            builder.AppendLine($"{indent}{mergeId} --> {relayId}");
        }

        foreach (var edge in partition.Edges)
        {
            if (consumedEdgeKeys.Contains(GetEdgeKey(edge)))
            {
                continue;
            }

            var fromId = GetMermaidEntityId(edge.From.Id);
            var toId = GetMermaidEntityId(edge.To.Id);
            var slotLabel = GetSlotLabel(edge.To, edge.InputIndex);
            var edgeState = ToConnectionState(edge.From.State);

            if (!string.IsNullOrEmpty(slotLabel))
            {
                AppendEdge(builder, indent, fromId, toId, EscapeMermaidText(slotLabel), edgeState);
            }
            else
            {
                AppendEdge(builder, indent, fromId, toId, null, edgeState);
            }
        }

        return builder.ToString();
    }

    static void AppendEdge(StringBuilder builder, string indent, string fromId, string toId, string? label, HttpAutomationConnectionState state)
    {
        var currentEdgeId = GetMermaidEdgeId(fromId, toId);
        if (!string.IsNullOrEmpty(label))
        {
            builder.AppendLine($"{indent}{fromId} {currentEdgeId}@-->|{label}| {toId}");
        }
        else
        {
            builder.AppendLine($"{indent}{fromId} {currentEdgeId}@--> {toId}");
        }

        builder.AppendLine($"{indent}class {currentEdgeId} {GetEdgeStateClass(state)};");
    }

    static string GetMermaidEdgeId(string fromId, string toId) => $"Edge_{fromId}_{toId}";

    string? GetSlotLabel(ClientAutomatorNode targetNode, int? inputIndex)
    {
        if (!inputIndex.HasValue)
        {
            return null;
        }

        var automationBuilding = targetNode.AutomationBuilding;
        if (automationBuilding is null)
        {
            return $"IN{inputIndex.Value + 1}";
        }

        var slot = automationBuilding.InputSlots.FirstOrDefault(s => s.Index == inputIndex.Value);
        return slot is null ? $"IN{inputIndex.Value + 1}" : slot.Name;
    }

    static string GetEdgeKey(ClientAutomatorEdge edge)
        => $"{edge.From.Id:N}>{edge.To.Id:N}>{edge.InputIndex?.ToString() ?? "-"}";

    static string? GetRelayOperatorLabel(ClientAutomatorNode node)
    {
        if (node.AutomationBuilding is null || node.AutomationSettings is not RelaySettingsModel relaySettings)
        {
            return null;
        }

        return node.AutomationBuilding.GetOperatorLabel(relaySettings);
    }

    static string GetMermaidEntityId(Guid id) => $"Entity_{id:N}";

    static string GetMermaidOperatorId(Guid targetEntityId) => $"Operator_{targetEntityId:N}";

    static string EscapeMermaidText(string text)
        => text
            .Replace("\\", "\\\\", StringComparison.Ordinal)
            .Replace("\"", "\\\"", StringComparison.Ordinal)
            .Replace("|", "\\|", StringComparison.Ordinal)
            .Replace("\n", "<br/>", StringComparison.Ordinal)
            .Replace("\r", "", StringComparison.Ordinal);

    static string GetAutomatorTypeLabel(ClientAutomatorNode node)
    {
        if (node.AutomationBuilding is not null)
        {
            return node.AutomationBuilding.DisplayTypeName;
        }

        if (node.Kind == HttpAutomatorKind.Terminal)
        {
            return "Terminal";
        }

        return "Unknown Transmitter";
    }

    static string GetStateClass(HttpAutomatorState state) => state switch
    {
        HttpAutomatorState.On => "state_on",
        HttpAutomatorState.Off => "state_off",
        HttpAutomatorState.Error => "state_error",
        _ => "state_off",
    };

    static string GetEdgeStateClass(HttpAutomationConnectionState state) => state switch
    {
        HttpAutomationConnectionState.On => "edge_state_on",
        HttpAutomationConnectionState.Off => "edge_state_off",
        HttpAutomationConnectionState.Disconnected => "edge_state_disconnected",
        _ => "edge_state_off",
    };

    static HttpAutomationConnectionState ToConnectionState(HttpAutomatorState state) => state switch
    {
        HttpAutomatorState.On => HttpAutomationConnectionState.On,
        HttpAutomatorState.Off => HttpAutomationConnectionState.Off,
        HttpAutomatorState.Error => HttpAutomationConnectionState.Disconnected,
        _ => HttpAutomationConnectionState.Off,
    };

    static string? GetAutomatorValueLabel(ClientAutomatorNode node)
        => node.AutomationBuilding is null || node.AutomationSettings is null
        ? null
        : node.AutomationBuilding.GetValueLabel(node.AutomationSettings);
}
