namespace SuperCursor.Services.ObjectDescribers;

public class MechanicalNodeDescriber(ILoc t) : BaseObjectDescriber<MechanicalNode>
{

    protected override void DescribeComponent(StringBuilder builder, MechanicalNode component)
    {
        if (!component || !component.Enabled) { return; }

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

        builder.AppendLine(t.T(NetworkFragmentService.NetworkPowerLocKey, 
            power.Value.PowerSupply,
            $"{power.Value.PowerDemand} {t.T(NetworkFragmentService.PowerSymbolLocKey)}").Indent());
    }

}
