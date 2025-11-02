namespace SuperCursor.Services.ObjectDescribers;

public abstract class BaseObjectDescriber<T> : IObjectDescriber where T : BaseComponent
{

    public void Describe(StringBuilder builder, BaseComponent component)
    {
        if (typeof(T) == typeof(BaseComponent))
        {
            DescribeComponent(builder, (T)component);
        }
        else
        {
            var comp = component.GetComponent<T>();
            if (comp is null) { return; }

            DescribeComponent(builder, comp);
        }

    }

    protected abstract void DescribeComponent(StringBuilder builder, T component);

}