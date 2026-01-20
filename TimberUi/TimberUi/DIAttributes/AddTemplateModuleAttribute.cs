namespace TimberUi;

[AttributeUsage(AttributeTargets.Class, Inherited = false, AllowMultiple = true)]
public class AddTemplateModuleAttribute(Type subject) : Attribute
{

    public Type Subject { get; } = subject;
    public bool AlsoBindTransient { get; init; } = true;

}
