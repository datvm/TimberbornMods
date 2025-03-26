
namespace RealLights.Components;

public class ColorHandler : JsonConverter
{
    public override bool CanConvert(Type objectType)
    {
        return objectType == typeof(Color) || objectType == typeof(Color?);
    }

    public override object? ReadJson(JsonReader reader, Type objectType, object? existingValue, JsonSerializer serializer)
    {
        try
        {
            if (reader.TokenType == JsonToken.Null)
            {
                return null;
            }
            else
            {
                return ColorUtility.TryParseHtmlString("#" + reader.Value, out Color loadedColor) ? loadedColor : 
                    throw new JsonSerializationException("Failed to parse color: " + reader.Value);
            }
        }
        catch (Exception ex)
        {
            Debug.LogError($"Failed to parse color {objectType} : {ex.Message}");
            return null;
        }
    }

    public override void WriteJson(JsonWriter writer, object? value, JsonSerializer serializer)
    {
        if (value is Color c)
        {
            writer.WriteValue(ColorUtility.ToHtmlStringRGBA(c));
        }
        else
        {
            writer.WriteNull();
        }
    }
}
