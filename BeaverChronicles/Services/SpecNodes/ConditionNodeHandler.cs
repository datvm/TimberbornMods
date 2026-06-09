namespace BeaverChronicles.Services.SpecNodes;

[MultiBind(typeof(ISpecNodeHandler))]
public class ConditionNodeHandler : NodeHandlerBase<ConditionData>
{
    public const string NodeType = "Condition";
    public override string ForType => NodeType;

    protected override string? InternalHandleNode(ConditionData data, ChronicleEventNodeSpec node, SpecChronicleEventController controller) 
        => controller.EvaluateConditionNode(node.Id)
            ? data.FulfilledNodeId
            : data.FailedNodeId;

}
