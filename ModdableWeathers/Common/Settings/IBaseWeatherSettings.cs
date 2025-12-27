
namespace ModdableWeathers.Common.Settings;

public interface IBaseWeatherSettings
{
    static readonly Dictionary<Type, ImmutableArray<NamedPropertyInfo>> TypeProperties = [];

    ImmutableArray<NamedPropertyInfo> Properties => GetPropertiesFor(GetType());

    void Deserialize(JObject json) => DefaultDeserialize(this, json);
    JObject Serialize() => DefaultSerialize(this);

    static void DefaultDeserialize(IBaseWeatherSettings settings, JObject json)
    {
        foreach (var (_, prop) in settings.Properties)
        {
            if (json.TryGetValue(prop.Name, out var value))
            {
                prop.SetValue(settings, value.ToObject(prop.PropertyType));
            }
        }
    }

    static JObject DefaultSerialize(IBaseWeatherSettings settings)
    {
        JObject values = [];

        foreach (var (_, prop) in settings.Properties)
        {
            values[prop.Name] = JToken.FromObject(prop.GetValue(settings));
        }

        return values;
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