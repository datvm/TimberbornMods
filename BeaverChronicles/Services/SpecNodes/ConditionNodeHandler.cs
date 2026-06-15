namespace BeaverChronicles.Services.SpecNodes;

[MultiBind(typeof(ISpecNodeHandler))]
public class ConditionNodeHandler : NodeHandlerBase<ConditionData>
{
    public const string NodeType = "Condition";
    public override string ForType => NodeType;

    protected override string? InternalHandleNode(ConditionData data, ChronicleEventNodeSpec node, SpecChronicleEventController controller)
    {
        if (controller.EvaluateConditionNode(node.Id))
        {
            node.LogVerbose(() => $"Evaluated to true, going to {data.FulfilledNodeId}.");
            return data.FulfilledNodeId;
        }
        else
        {
            node.LogVerbose(() => $"Evaluated to false, going to {data.FailedNodeId}.");
            return data.FailedNodeId;
        }
    }
}
