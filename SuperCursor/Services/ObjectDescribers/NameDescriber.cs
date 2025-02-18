namespace SuperCursor.Services.ObjectDescribers;

[ServicePriority(0)]
public class NameDescriber(EntityBadgeService badge) : BaseObjectDescriber<BaseComponent>
{

    protected override void DescribeComponent(StringBuilder builder, BaseComponent component)
    {
        builder.AppendLine(badge.GetEntityName(component).Bold().Bigger());
    }

}
