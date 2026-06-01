using ModdableTimberborn.SerializationSystem;

namespace Timberborn.SerializationSystem;

public static class ModdableTimberbornSerializationExtensions
{
    static readonly JsonSerializerSettings settings = new()
    {
        Converters = [SerializedObjectJsonConverter.Instance],
    };

    public static SerializedObject? DeserializeSerializedObject(string json)
        => JsonConvert.DeserializeObject<SerializedObject>(json, settings);

    extension(SerializedObject obj)
    {
        public string SerializeToJson() => JsonConvert.SerializeObject(obj, settings);
        public T? DeserializeTo<T>(JsonSerializerSettings? settings = null) => JsonConvert.DeserializeObject<T>(obj.SerializeToJson(), settings);
    }

}
