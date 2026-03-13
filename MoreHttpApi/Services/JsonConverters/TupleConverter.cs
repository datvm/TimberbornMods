namespace MoreHttpApi.Services.JsonConverters;

public class TupleConverter : JsonConverter
{
    public override bool CanConvert(Type objectType) => typeof(ITuple).IsAssignableFrom(objectType);

    public override object? ReadJson(JsonReader reader, Type objectType, object? existingValue, JsonSerializer serializer)
    {
        throw new NotImplementedException();
    }

    public override void WriteJson(JsonWriter writer, object? value, JsonSerializer serializer)
    {
        if (value == default)
        {
            writer.WriteNull();
            return;
        }

        writer.WriteStartArray();
        var t = (ITuple)value;
        var len = t.Length;

        for (int i = 0; i < len; i++)
        {
            var e = t[i];
            writer.WriteValue(e);
        }

        writer.WriteEndArray();
    }
}
