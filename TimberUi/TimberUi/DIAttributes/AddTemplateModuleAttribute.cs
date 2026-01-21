namespace TimberUi;

[Obsolete($"Use {nameof(AddTemplateModule2Attribute)} instead")]
[AttributeUsage(AttributeTargets.Class, Inherited = false, AllowMultiple = true)]
public class AddTemplateModuleAttribute(Type subject) : Attribute
{

    public Type Subject { get; } = subject;
    public bool AlsoBindTransient { get; init; } = true;

}


[AttributeUsage(AttributeTargets.Class, Inherited = false, AllowMultiple = true)]
public class AddTemplateModule2Attribute(Type subject) : Attribute
{

    public Type Subject { get; } = subject;
    public bool AlsoBindTransient { get; init; } = true;
    public BindAttributeContext Contexts { get; init; } = BindAttributeContext.Game;

}