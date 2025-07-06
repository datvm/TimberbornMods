using Newtonsoft.Json.Serialization;

namespace TimberDump.Services.Json;

public class JsonIgnoreUnityObjectContract : DefaultContractResolver
{

    public JsonIgnoreUnityObjectContract()
    {
#pragma warning disable CS0618 // Type or member is obsolete
        DefaultMembersSearchFlags = BindingFlags.Public | BindingFlags.NonPublic | BindingFlags.Instance;
#pragma warning restore CS0618 // Type or member is obsolete
    }

    protected override IList<JsonProperty> CreateProperties(Type type, MemberSerialization memberSerialization)
    {
        var properties = base.CreateProperties(type, memberSerialization);

        return [.. properties.Where(p => p.PropertyType is not null && p.DeclaringType is not null && p.Writable)];
    }

}
