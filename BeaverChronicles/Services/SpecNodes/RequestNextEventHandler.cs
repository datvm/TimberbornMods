namespace BeaverChronicles.Services.SpecNodes;

[MultiBind(typeof(ISpecNodeHandler))]
public class RequestNextEventHandler : NodeHandlerBase<RequestNextEventData>
{
    public override string ForType => "RequestNextEvent";

    protected override string? InternalHandleNode(RequestNextEventData data, ChronicleEventNodeSpec node, SpecChronicleEventController controller)
    {
        node.LogVerbose(() => $"Requesting next event to be: '{controller.FormatText(data.Id)}'.");
        controller.ActiveContext.RequestNextEvent(controller.FormatText(data.Id));
        return node.NextNodeId;
    }
}
