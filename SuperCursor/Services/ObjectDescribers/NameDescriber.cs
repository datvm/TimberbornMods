namespace SuperCursor.Services.ObjectDescribers;

[ServicePriority(0)]
public class NameDescriber : BaseObjectDescriber<BaseComponent>
{

    protected override void DescribeComponent(StringBuilder builder, BaseComponent component)
    {
        var name = component.GetComponent<NamedEntity>();
        if (!name) { return; }

        builder.AppendLine(name.EntityName.Bold().Bigger());
    }

}
