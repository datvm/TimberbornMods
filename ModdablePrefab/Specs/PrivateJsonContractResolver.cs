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

    protected override JsonProperty CreateProperty(MemberInfo member, MemberSerialization memberSerialization)
    {
        var prop = base.CreateProperty(member, memberSerialization);

        if (member is FieldInfo ||
            (member is PropertyInfo propInfo && propInfo.CanWrite))
        {
            prop.Readable = true;
            prop.Writable = true;
        }

        return prop;
    }

}
