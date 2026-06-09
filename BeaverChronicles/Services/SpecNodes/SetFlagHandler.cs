namespace BeaverChronicles.Services.SpecNodes;

[MultiBind(typeof(ISpecNodeHandler))]
public class SetFlagHandler : NodeHandlerBase<SetFlagData>
{
    public override string ForType => "SetFlag";

    protected override string? InternalHandleNode(SetFlagData data, ChronicleEventNodeSpec node, SpecChronicleEventController controller)
    {
        var name = controller.FormatText(data.Name);
        if (string.IsNullOrEmpty(name))
        {
            throw new InvalidOperationException($"Event {controller.Event.Id} node {node.Id}: flag name is missing.");
        }

        if (data.State)
        {
            controller.HelperCollection.Flags.AddFlag(name);
        }
        else
        {
            controller.HelperCollection.Flags.RemoveFlag(name);
        }

        return node.NextNodeId;
    }
}
