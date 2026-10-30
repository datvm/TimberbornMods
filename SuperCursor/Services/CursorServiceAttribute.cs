namespace SuperCursor.Services;

[AttributeUsage(AttributeTargets.Class, Inherited = false)]
public class CursorServiceAttribute(int priority, bool gameOnly = false) : Attribute
{

    public int Priority => priority;
    public bool GameOnly => gameOnly;

}
