namespace SuperCursor.Services.ObjectDescribers;

[ServicePriority(1)]
public class WaterSourceDescriber(ILoc t) : BaseObjectDescriber<WaterSource>
{
    protected override void DescribeComponent(StringBuilder builder, WaterSource component)
    {

        if (component.CurrentStrength == component.SpecifiedStrength)
        {
            builder.AppendLine(t.T("LV.SC.WaterStrength", component.CurrentStrength));
        }
        else
        {
            builder.AppendLine(t.T("LV.SC.WaterSpecifiedStrength", component.CurrentStrength, component.SpecifiedStrength));
        }

        builder.AppendLine(t.T("LV.SC.WaterContamination", component.Contamination.ToString("F0")));
    }
}
