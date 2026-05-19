namespace TimberLive.Models;

/// <summary>
/// Client-side view of automations grouped into connected components (partitions).
/// </summary>
public class ClientAutomationMap
{
    /// <summary>Connected component partitions, sorted by size (largest first).</summary>
    public List<ClientAutomationPartition> Partitions { get; } = [];

    /// <summary>All buildings referenced by any automator, for label lookup.</summary>
    public Dictionary<Guid, HttpBuilding> Buildings { get; } = [];

    /// <summary>Lookup from automator/node ID to its partition ID.</summary>
    public Dictionary<Guid, ClientAutomationPartition> PartitionByAutomatorId { get; } = [];
}

public class ClientAutomationPartition
{
    public int Id { get; set; }
    public List<ClientAutomatorNode> Automators { get; } = [];
    public List<ClientAutomatorEdge> Edges { get; } = [];
    public bool IsIsolated => Automators.Count == 1 && Edges.Count == 0;
}

public interface IClientAutomatorNode
{
    Guid Id { get; }
    bool IsCyclicOrBlocked { get; }
    HttpAutomatorKind Kind { get; }
    string Label { get; }
    HttpAutomatorState State { get; }
}

public interface IClientAutomatorNode<TSettings> : IClientAutomatorNode
{
    TSettings Settings { get; }
}

public class ClientAutomatorNode : IClientAutomatorNode
{
    public Guid Id { get; set; }
    public HttpAutomatorKind Kind { get; set; }
    public HttpAutomatorState State { get; set; }
    public bool IsCyclicOrBlocked { get; set; }
    public string Label { get; set; } = string.Empty;
    public IClientAutomationBuilding? AutomationBuilding { get; set; }
    public object? AutomationSettings { get; set; }
}

public class ClientAutomatorNode<TSettings> : ClientAutomatorNode, IClientAutomatorNode<TSettings>
{
    public required TSettings Settings { get; init; }
}

public class ClientAutomatorEdge
{
    public required ClientAutomatorNode From { get; set; }
    public required ClientAutomatorNode To { get; set; }
    public HttpAutomationConnectionState State { get; set; }
    public int? InputIndex { get; set; }
}