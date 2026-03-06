
namespace MoreHttpApi.Services.JsonConverters;

public class BlueprintContractResolver : DefaultContractResolver
{
    static readonly FrozenSet<Type> IgnoreTypes = [typeof(FlippedSprite), typeof(UISprite), typeof(Sprite)];

    // Ignore certain type
    protected override IList<JsonProperty> CreateProperties(Type type, MemberSerialization memberSerialization)
    {
        var result = base.CreateProperties(type, memberSerialization);

        for (int i = result.Count - 1; i >= 0; i--)
        {
            var prop = result[i];
            
            if (IgnoreTypes.Contains(prop.PropertyType!))
            {
                result.RemoveAt(i);
            }
        }

        return result;
    }

}
