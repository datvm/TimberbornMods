namespace MoreHttpApi.Services.JsonConverters;

public class UnityValuesConverter : JsonConverter
{

    static readonly FrozenDictionary<Type, MethodInfo> SupportedTypes;

    static UnityValuesConverter()
    {
        Dictionary<Type, MethodInfo> supportedTypes = [];

        // int takes precedent
        var intType = typeof(SerializableInts);
        foreach (var (m, t) in GetImplicitTypes(intType))
        {
            supportedTypes[t] = m;
        }

        var floatType = typeof(SerializableFloats);
        foreach (var (m, t) in GetImplicitTypes(floatType))
        {
            if (!supportedTypes.ContainsKey(t))
            {
                supportedTypes[t] = m;
            }   
        }

        supportedTypes.Remove(intType);
        supportedTypes.Remove(floatType);

        SupportedTypes = supportedTypes.ToFrozenDictionary();
    }

    static IEnumerable<(MethodInfo, Type)> GetImplicitTypes(Type t)
    {
        foreach (var m in t.GetMethods())
        {
            if (m.IsStatic && m.IsPublic && m.Name == "op_Implicit" && m.ReturnType == t)
            {
                var type = m.GetParameters()[0].ParameterType;
                yield return (m, type);
            }
        }
    }

    public override bool CanConvert(Type objectType) => SupportedTypes.ContainsKey(objectType);

    public override object? ReadJson(JsonReader reader, Type objectType, object? existingValue, JsonSerializer serializer) 
        => throw new NotImplementedException();

    public override void WriteJson(JsonWriter writer, object? value, JsonSerializer serializer)
    {
        if (value is null)
        {
            writer.WriteNull();
            return;
        }

        var type = value.GetType();
        var method = SupportedTypes[type];

        var converted = method.Invoke(null, [value]);
        serializer.Serialize(writer, converted);
    }
}
