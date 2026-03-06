global using System.Reflection;

namespace MoreHttpApi.Shared;

[AttributeUsage(AttributeTargets.Class, AllowMultiple = false)]
public class ComponentSpecAttribute(string Name) : Attribute 
{
    public string Name { get; } = Name;
}

public abstract record ParsedComponentSpec
{

    public static readonly IReadOnlyDictionary<Type, string> ModelTypeMapping;

    public HttpBlueprint Blueprint { get; set; } = null!;

    static ParsedComponentSpec()
    {
        Dictionary<Type, string> mapping = [];

        var types = AppDomain.CurrentDomain.GetAssemblies()
            .SelectMany(a => a.GetTypes())
            .Where(t => typeof(ParsedComponentSpec).IsAssignableFrom(t) && !t.IsAbstract);

        foreach ( var type in types )
        {
            var attr = type.GetCustomAttribute<ComponentSpecAttribute>();
            if (attr != null)
            {
                mapping[type] = attr.Name;
            }
        }

        ModelTypeMapping = mapping;
    }

}
