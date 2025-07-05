using Newtonsoft.Json.Serialization;

namespace TimberDump.Services;

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
        if (IsUnityObjectOrDerived(type)) { return []; }

        var properties = base.CreateProperties(type, memberSerialization);

        return [.. properties.Where(p =>
        {
            if (p.PropertyType is null || p.DeclaringType is null || !p.Writable) { return false; }

            var isUnityObj = IsUnityObjectOrDerived(p.PropertyType);

            if (isUnityObj)
            {
                Debug.Log($"{p.DeclaringType.FullName}.{p.PropertyName} is a Unity Object ({p.PropertyType.FullName}), ignoring it.");
            }

            return !isUnityObj;
        })];
    }

    private static bool IsUnityObjectOrDerived(Type? type)
    {
        if (type is null)
        {
            return false;
        }

        // Direct UnityEngine.Object or derived
        if (typeof(UnityEngine.Object).IsAssignableFrom(type))
        {
            return true;
        }

        // Arrays
        if (type.IsArray)
        {
            var elementType = type.GetElementType();
            if (IsUnityObjectOrDerived(elementType))
            {
                return true;
            }
        }

        // IEnumerable<T> where T : UnityEngine.Object
        if (type.IsGenericType)
        {
            if (typeof(IEnumerable).IsAssignableFrom(type))
            {
                var args = type.GetGenericArguments();
                if (args.Length == 1 && IsUnityObjectOrDerived(args[0]))
                {
                    return true;
                }
            }
        }

        // Check interfaces for IEnumerable<T> where T : UnityEngine.Object
        foreach (var iface in type.GetInterfaces())
        {
            if (iface.IsGenericType && iface.GetGenericTypeDefinition() == typeof(IEnumerable<>))
            {
                var arg = iface.GetGenericArguments()[0];
                if (IsUnityObjectOrDerived(arg))
                {
                    return true;
                }
            }
        }

        return false;
    }
}
