namespace SuperCursor.Services;

[AttributeUsage(AttributeTargets.Class, Inherited = false)]
public class ServicePriorityAttribute(int priority) : Attribute
{

    public int Priority => priority;

}
