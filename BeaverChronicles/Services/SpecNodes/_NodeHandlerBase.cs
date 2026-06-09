namespace BeaverChronicles.Services.SpecNodes;

public abstract class NodeHandlerBase : ISpecNodeHandler
{
    public abstract string ForType { get; }

    public void HandleNode(ChronicleEventNodeSpec node, SpecChronicleEventController controller)
    {
        var nextNodeId = InternalHandleNode(node, controller);
        controller.TriggerNode(nextNodeId);
    }

    protected abstract string? InternalHandleNode(ChronicleEventNodeSpec node, SpecChronicleEventController controller);

    public virtual void RestoreGameState(ChronicleEventNodeSpec node, SpecChronicleEventController controller) 
        => throw ThrowRestoreGameStateException(node.Type);

    public static InvalidOperationException ThrowRestoreGameStateException(string nodeType)
        => new InvalidOperationException($"The game should not be saved during a {nodeType} node");
}

public abstract class NodeHandlerBase<T> : NodeHandlerBase where T : class
{

    protected override string? InternalHandleNode(ChronicleEventNodeSpec node, SpecChronicleEventController controller)
    {
        var data = node.GetData<T>();
        return InternalHandleNode(data, node, controller);
    }

    protected abstract string? InternalHandleNode(T data, ChronicleEventNodeSpec node, SpecChronicleEventController controller);

}