namespace MoreHttpApi.Services.JsonConverters;

public class AssetRefConverter : JsonConverter
{
    static readonly Type TypeDefinition = typeof(AssetRef<>);

    public override bool CanConvert(Type objectType) 
        => objectType.IsGenericType && objectType.GetGenericTypeDefinition() == TypeDefinition;

    public override object? ReadJson(JsonReader reader, Type objectType, object? existingValue, JsonSerializer serializer) 
        => throw new NotImplementedException();

    public override void WriteJson(JsonWriter writer, object? value, JsonSerializer serializer)
    {
        if (value is null)
        {
            writer.WriteNull();
            return;
        }

        var pathProp = value.GetType().GetProperty(nameof(AssetRef<>.Path));
        var path = pathProp.GetValue(value) as string;
        writer.WriteValue(path);
    }
}
