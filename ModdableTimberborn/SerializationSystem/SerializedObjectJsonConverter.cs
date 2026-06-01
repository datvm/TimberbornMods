using Newtonsoft.Json.Linq;

namespace ModdableTimberborn.SerializationSystem;

public sealed class SerializedObjectJsonConverter : JsonConverter<SerializedObject>
{
    public static readonly SerializedObjectJsonConverter Instance = new();

    public override void WriteJson(JsonWriter writer, SerializedObject? value, JsonSerializer serializer)
    {
        WriteValue(writer, value, serializer);
    }

    public override SerializedObject? ReadJson(
        JsonReader reader,
        Type objectType,
        SerializedObject? existingValue,
        bool hasExistingValue,
        JsonSerializer serializer)
    {
        if (reader.TokenType == JsonToken.Null)
        {
            return null;
        }

        var token = JToken.Load(reader);
        return ReadObject(token);
    }

    static void WriteValue(JsonWriter writer, object? value, JsonSerializer serializer)
    {
        switch (value)
        {
            case null:
                writer.WriteNull();
                break;

            case SerializedObject serializedObject:
                writer.WriteStartObject();

                foreach (var propertyName in serializedObject.Properties())
                {
                    writer.WritePropertyName(propertyName);
                    WriteValue(writer, serializedObject.GetSerialized(propertyName), serializer);
                }

                writer.WriteEndObject();
                break;

            case Array array:
                writer.WriteStartArray();

                foreach (var item in array)
                {
                    WriteValue(writer, item, serializer);
                }

                writer.WriteEndArray();
                break;

            default:
                serializer.Serialize(writer, value);
                break;
        }
    }

    static SerializedObject ReadObject(JToken token)
    {
        if (token.Type != JTokenType.Object)
        {
            throw new JsonSerializationException($"Expected object, got {token.Type}.");
        }

        var properties = new Dictionary<string, object?>();

        foreach (var property in token.Children<JProperty>())
        {
            properties[property.Name] = ReadValue(property.Value);
        }

        return new SerializedObject(properties);
    }

    static object? ReadValue(JToken token)
    {
        return token.Type switch
        {
            JTokenType.Object => ReadObject(token),
            JTokenType.Array => ReadArray(token),
            JTokenType.Integer => token.Value<int>(),
            JTokenType.Float => token.Value<float>(),
            JTokenType.Boolean => token.Value<bool>(),
            JTokenType.String => token.Value<string>(),
            JTokenType.Null => null,
            _ => throw new JsonSerializationException($"Unsupported token type: {token.Type}")
        };
    }

    static object?[] ReadArray(JToken token)
    {
        var values = new List<object?>();

        foreach (var child in token.Children())
        {
            values.Add(ReadValue(child));
        }

        return [.. values];
    }
}