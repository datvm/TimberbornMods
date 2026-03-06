namespace MoreHttpApi.Services.JsonConverters;

public class ComponentSpecConverter : JsonConverter<ComponentSpec>
{
    static readonly Dictionary<Type, ImmutableArray<PropertyInfo>> cachedProperties = [];

    public override ComponentSpec? ReadJson(JsonReader reader, Type objectType, ComponentSpec? existingValue, bool hasExistingValue, JsonSerializer serializer)
        => throw new NotImplementedException();

    public override void WriteJson(JsonWriter writer, ComponentSpec? value, JsonSerializer serializer)
    {
        if (value is null)
        {
            writer.WriteNull();
            return;
        }

        var type = value.GetType();
        var props = cachedProperties.GetOrAdd(type, () => [..GetProperties(type)]);

        writer.WriteStartObject();

        foreach (var prop in props)
        {
            writer.WritePropertyName(prop.Name);
            
            var propValue = prop.GetValue(value);
            serializer.Serialize(writer, propValue);
        }

        writer.WriteEndObject();
    }

    static IEnumerable<PropertyInfo> GetProperties(Type type)
    {
        foreach (var prop in type.GetProperties(SerializableTypeExtensions.AllInstanceFlag))
        {
            if (prop.GetCustomAttribute<SerializeAttribute>() is not null)
            {
                yield return prop;
            }
        }
    }

}
