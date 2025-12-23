namespace ModdableWeathers.Patches;

[AttributeUsage(AttributeTargets.Class, Inherited = false, AllowMultiple = false)]
public class ReplaceSingletonAttribute(Type? replaced = null) : Attribute
{

    public Type? Replaced { get; } = replaced;

}

[AttributeUsage(AttributeTargets.Class, Inherited = false, AllowMultiple = false)]
public class HasPatchAttribute : Attribute { }

[AttributeUsage(AttributeTargets.Property, Inherited = false, AllowMultiple = false)]
public class ReplacePropertyAttribute(string? replacedName = null) : Attribute
{
    public string? ReplacedName { get; } = replacedName;
}

[AttributeUsage(AttributeTargets.Method, Inherited = false, AllowMultiple = false)]
public class ReplaceMethodAttribute(string? replacedName = null) : Attribute
{
    public string? ReplacedName { get; } = replacedName;
}

[AttributeUsage(AttributeTargets.Class, Inherited = false, AllowMultiple = true)]
public class BypassMethodsAttribute(params string[] methodNames) : Attribute
{ 
    public HashSet<string> MethodNames { get; } = [..methodNames];
}

public interface IThrowAttribute
{
    HashSet<string>? Names { get; }
    Type? ExceptionType { get; set; }
    string? ExceptionMember { get; set; }
}

[AttributeUsage(AttributeTargets.Class, Inherited = false, AllowMultiple = true)]
public class ThrowMethodsAttribute(params string[] methodNames) : Attribute, IThrowAttribute
{
    public HashSet<string> Names { get; } = [..methodNames];
    public Type? ExceptionType { get; set; }
    public string? ExceptionMember { get; set; }
}

[AttributeUsage(AttributeTargets.Class, Inherited = false, AllowMultiple = true)]
public class ThrowPropertiesAttribute(params string[] propertyNames) : Attribute, IThrowAttribute
{
    public HashSet<string> Names { get; } = [.. propertyNames];
    public Type? ExceptionType { get; set; }
    public string? ExceptionMember { get; set; }
}