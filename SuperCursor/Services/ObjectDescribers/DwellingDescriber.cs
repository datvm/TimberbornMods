

namespace SuperCursor.Services.ObjectDescribers;

public class DwellingDescriber(ILoc t) : BaseObjectDescriber<Dwelling>
{
    protected override void DescribeComponent(StringBuilder builder, Dwelling component)
    {
        builder.AppendLine($"{t.T("Dwelling.Dwellers").Bold()}: {component.NumberOfDwellers}/{component.MaxBeavers}");

        if (component.NumberOfDwellers > 0)
        {
            builder.AppendLine($"  {t.T("LV.SC.Dwellers", component.NumberOfAdultDwellers, component.NumberOfChildDwellers)}");
        }
    }
}
