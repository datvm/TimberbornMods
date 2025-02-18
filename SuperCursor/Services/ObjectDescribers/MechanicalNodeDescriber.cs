
namespace SuperCursor.Services.ObjectDescribers;

public class MechanicalNodeDescriber(ILoc t) : BaseObjectDescriber<MechanicalNode>
{

    protected override void DescribeComponent(StringBuilder builder, MechanicalNode component)
    {
        builder.AppendLine(t.T("ToolGroups.Power").Bold());

        if (component.IsGenerator)
        {
            builder.AppendLine(MechanicalNodeTextFormatter.FormatGeneratorText(t, component).Indent());
        }

        if (component.IsConsumer)
        {
            builder.AppendLine(MechanicalNodeTextFormatter.FormatConsumerText(t, component).Indent());
        }

        var power = component.Graph.CurrentPower;
        builder.AppendLine(t.T("Mechanical.NetworkPower", power.PowerSupply, power.PowerDemand).Indent());
    }

}
