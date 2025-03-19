global using Newtonsoft.Json.Serialization;

namespace ModdablePrefab;

public class PrivateJsonContractResolver : DefaultContractResolver
{

    protected override List<MemberInfo> GetSerializableMembers(Type objectType)
    {
        var list = objectType.GetMembers(BindingFlags.Public | BindingFlags.NonPublic | BindingFlags.Instance | BindingFlags.Static)
            .Where(q => q.MemberType is MemberTypes.Field or MemberTypes.Property
                && (q is not PropertyInfo prop || prop.CanWrite))
            .ToList();

        return list;
    }

    protected override IList<JsonProperty> CreateProperties(Type type, MemberSerialization memberSerialization)
    {
        return [.. GetSerializableMembers(type)
            .Select(q =>
            {
                var prop = CreateProperty(q, memberSerialization);
                prop.Writable = true;
                prop.Readable = true;
                return prop;
            })];
    }

}
