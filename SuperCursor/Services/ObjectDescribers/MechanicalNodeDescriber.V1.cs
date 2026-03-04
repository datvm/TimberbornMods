namespace SuperCursor.Services.ObjectDescribers;

public class MechanicalNodeDescriber(ILoc t, MechanicalNodeTextFormatter formatter) : BaseObjectDescriber<MechanicalNode>
{

    protected override void DescribeComponent(StringBuilder builder, MechanicalNode component)
    {
        if (!component || !component.Enabled) { return; }

        var power = component.Graph;
        if (power is null) { return; }

        builder.AppendLine(t.T("ToolGroups.Power").Bold());

        if (component.IsGenerator)
        {
            builder.AppendLine(formatter.FormatGeneratorText(component).Indent());
        }

        if (component.IsConsumer)
        {
            builder.AppendLine(formatter.FormatConsumerText(component).Indent());
        }

        builder.AppendLine(t.T("Mechanical.NetworkPower", 
            power.PowerSupply,
            UnitFormatter.FormatPower(power.PowerDemand, t)).Indent());
    }

}
