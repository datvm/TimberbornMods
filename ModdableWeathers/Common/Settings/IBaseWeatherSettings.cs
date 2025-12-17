namespace ModdableWeathers.Common.Settings;

public interface IBaseWeatherSettings
{
    static readonly Dictionary<Type, ImmutableArray<NamedPropertyInfo>> TypeProperties = [];

    ImmutableArray<NamedPropertyInfo> Properties => GetPropertiesFor(GetType());

    void Deserialize(string serialized)
    {
        var values = JsonConvert.DeserializeObject<Dictionary<string, object?>>(serialized);
        if (values is null) { return; }

        foreach (var (_, prop) in Properties)
        {
            if (values.TryGetValue(prop.Name, out var value))
            {
                prop.SetValue(this, Convert.ChangeType(value, prop.PropertyType));
            }
        }
    }

    string Serialize()
    {
        Dictionary<string, object?> values = [];

        foreach (var (_, prop) in Properties)
        {
            values[prop.Name] = prop.GetValue(this);
        }

        return JsonConvert.SerializeObject(this, Formatting.Indented);
    }

    static ImmutableArray<NamedPropertyInfo> GetPropertiesFor(Type type)
    {
        if (TypeProperties.TryGetValue(type, out var properties)) { return properties; }

        List<NamedPropertyInfo> result = [];
        var props = type.GetProperties(BindingFlags.Public | BindingFlags.Instance);

        foreach (var prop in props)
        {
            if (!prop.CanRead || !prop.CanWrite) { continue; }

            var descAttr = prop.GetCustomAttribute<DescriptionAttribute>();
            if (descAttr is not null)
            {
                result.Add(new NamedPropertyInfo(descAttr.Description, prop));
            }
        }

        return TypeProperties[type] = [.. result];
    }

}

public readonly record struct NamedPropertyInfo(string Name, PropertyInfo Property);