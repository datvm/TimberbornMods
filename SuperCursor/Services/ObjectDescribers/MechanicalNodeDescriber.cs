
namespace SuperCursor.Services.ObjectDescribers;

public class MechanicalNodeDescriber(ILoc t) : BaseObjectDescriber<MechanicalNode>
{

    protected override void DescribeComponent(StringBuilder builder, MechanicalNode component)
    {
        if (component?.enabled != true) { return; }

        var power = component.Graph?.CurrentPower;
        if (power is null) { return; }

        builder.AppendLine(t.T("ToolGroups.Power").Bold());

        if (component.IsGenerator)
        {
            builder.AppendLine(MechanicalNodeTextFormatter.FormatGeneratorText(t, component).Indent());
        }

        if (component.IsConsumer)
        {
            builder.AppendLine(MechanicalNodeTextFormatter.FormatConsumerText(t, component).Indent());
        }

        builder.AppendLine(t.T("Mechanical.NetworkPower", power.Value.PowerSupply, power.Value.PowerDemand).Indent());
    }

}
