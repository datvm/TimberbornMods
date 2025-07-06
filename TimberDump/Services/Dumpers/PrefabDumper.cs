namespace TimberDump.Services.Dumpers;

public class PrefabDumper(IAssetLoader assets) : IJsonDumper
{
    static readonly Dictionary<Type, ImmutableArray<FieldInfo>> TypeCache = [];

    public string? Folder { get; } = "Objects";
    public int Order { get; }

    public IEnumerable<(string Name, Func<object?> Data)> GetDumpData()
    {
        var objs = assets.LoadAll<GameObject>("");

        foreach (var obj in objs)
        {
            if (!obj.Asset) { continue; }

            var prefabSpec = obj.Asset.GetComponent<PrefabSpec>();
            if (!prefabSpec) { continue; }

            yield return (prefabSpec.Name, () => DumpObject(obj.Asset));
        }
    }

    static object? DumpObject(GameObject obj)
    {
        Dictionary<string, object> values = [];
        var comps = obj.GetComponents<BaseComponent>();

        foreach (var comp in comps)
        {
            var t = comp.GetType();
            var fields = TypeCache.GetOrAdd(t, () => GetSerializedFields(t));
            if (fields.Length == 0) { continue; }

            Dictionary<string, object> properties = [];
            foreach (var field in fields)
            {
                var value = field.GetValue(comp);
                properties[field.Name] = value;
            }
            values[t.Name] = properties;
        }

        return values.Count == 0 ? null : values;
    }

    static ImmutableArray<FieldInfo> GetSerializedFields(Type type) =>
        [.. type.GetFields(BindingFlags.Public | BindingFlags.NonPublic | BindingFlags.Instance)
            .Where(f => f.GetCustomAttribute<SerializeField>() != null)];

    public void Dump(string folder)
    {
        throw new NotImplementedException();
    }
}
