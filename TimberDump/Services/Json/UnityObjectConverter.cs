
using TimberDump.Patches;

namespace TimberDump.Services.Json;

public class UnityObjectConverter : JsonConverter
{

    public override bool CanConvert(Type objectType)
    {
        return typeof(UnityEngine.Object).IsAssignableFrom(objectType);
    }

    public override object? ReadJson(JsonReader reader, Type objectType, object? existingValue, JsonSerializer serializer)
    {
        throw new NotImplementedException();
    }

    public override void WriteJson(JsonWriter writer, object? value, JsonSerializer serializer)
    {
        if (value is not UnityEngine.Object obj)
        {
            goto WRITENULL;
        }

        if (AssetLoaderPatches.AssetPaths.TryGetValue(obj, out var path))
        {
            writer.WriteValue(path);
            return;
        }

    WRITENULL:
        writer.WriteNull();

    }
}
