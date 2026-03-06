namespace MoreHttpApi.Services.JsonConverters;

public class LocalizedTextConverter : JsonConverter<LocalizedText>
{
    public override LocalizedText? ReadJson(JsonReader reader, Type objectType, LocalizedText? existingValue, bool hasExistingValue, JsonSerializer serializer) => throw new NotImplementedException();

    public override void WriteJson(JsonWriter writer, LocalizedText? value, JsonSerializer serializer)
    {
        writer.WriteValue(value?.Value);
    }
}
