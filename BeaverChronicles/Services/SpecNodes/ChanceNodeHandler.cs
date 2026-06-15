namespace BeaverChronicles.Services.SpecNodes;

[MultiBind(typeof(ISpecNodeHandler))]
public class ChanceNodeHandler : NodeHandlerBase<ChanceData>
{
    public override string ForType => "Chance";

    protected override string? InternalHandleNode(ChanceData data, ChronicleEventNodeSpec node, SpecChronicleEventController controller)
    {
        var chance = controller.FormatTextFloat(data.Value);
        var nextNodeId = BeaverChroniclesUtils.Chance(chance)
            ? data.SuccessNodeId : data.FailNodeId;

        // Roll already printed by Chance function
        node.LogVerbose(() => $"Going to {nextNodeId}");
        return nextNodeId;
    }
}
