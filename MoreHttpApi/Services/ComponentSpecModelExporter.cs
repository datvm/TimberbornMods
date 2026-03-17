namespace MoreHttpApi.Services;

// Comment or uncomment this to disable/run the exporter.
//#warning Comment this out before release
//[BindSingleton(Contexts = BindAttributeContext.MainMenu)]
public class ComponentSpecModelExporter : ILoadableSingleton
{

    static readonly FrozenSet<Type> SkippedTypes = [typeof(FlippedSprite), typeof(UISprite)];
    static readonly FrozenSet<Type> SerializableIntTypes = [typeof(Vector3Int), typeof(Vector2Int), typeof(RectInt)];
    static readonly FrozenSet<Type> SerializableFloatTypes = [typeof(Vector3), typeof(Vector2), typeof(Quaternion), typeof(Rect), typeof(Vector4), typeof(Color)];
    static readonly FrozenSet<Type> StringTypes = [typeof(string), typeof(LocalizedText)];

    static readonly FrozenSet<string> IdProperties = ["Id", "SoundId", "CollectionId"];

    public void Load()
    {
        const string outputFolder = @"D:\Temp\Specs";

        if (Directory.Exists(outputFolder))
        {
            Directory.Delete(outputFolder, true);
        }
        Directory.CreateDirectory(outputFolder);

        var types = AppDomain.CurrentDomain.GetAssemblies()
            .SelectMany(a => a.GetTypes())
            .Where(t => t.IsClass && !t.IsAbstract && typeof(ComponentSpec).IsAssignableFrom(t) && t.Namespace.StartsWith(nameof(Timberborn)))
            .ToHashSet();

        HashSet<Type> additionalTypes = [.. types];
        HashSet<Type> pendingEnums = [];

        while (additionalTypes.Count > 0)
        {
            foreach (var t in additionalTypes.ToArray())
            {
                var props = t.GetProperties(AdvancedDeserializer.AllInstanceFlag)
                    .Where(p => p.GetCustomAttribute<SerializeAttribute>() is not null);

                List<(string, string)> outputProps = [];
                string? idProp = null;

                foreach (var p in props)
                {
                    var pType = p.PropertyType;

                    if (idProp is null)
                    {
                        if (IdProperties.Contains(p.Name))
                        {
                            if (p.PropertyType != typeof(string))
                            {
                                Debug.LogWarning($"[WARN] Skipping 'Id' property of type '{p.PropertyType.FullName}' in spec '{t.FullName}' because it's not a string");
                            }
                            else
                            {
                                idProp = p.Name;
                                TimberUiUtils.LogVerbose(() => $"Identified '{p.Name}' as ID property for spec '{t.FullName}'");
                            }
                        }
                        else if (p.Name.EndsWith("Id"))
                        {
                            Debug.LogWarning($"[WARN] Skipping '{p.Name}' property in spec '{t.FullName}' because it ends with 'Id' but is not named 'Id'");
                        }
                    }

                    try
                    {
                        var targetType = DeterminePropertyType(pType, types, additionalTypes, pendingEnums);
                        if (targetType is null) { continue; }

                        if (targetType.Contains('`')) { throw new NotSupportedException(); }

                        outputProps.Add((p.Name, targetType));
                    }
                    catch (NotSupportedException)
                    {
                        throw new NotSupportedException($"Property '{p.Name}' of type '{pType.FullName}' is not supported in spec '{t.FullName}'");
                    }
                }

                var propList = string.Join("," + Environment.NewLine, outputProps.Select(p => $"    {p.Item2} {p.Item1}"));

                var className = GetParsedTypeName(t);
                var filePath = Path.Combine(outputFolder, className + ".cs");

                if (t.IsGenericTypeDefinition)
                {
                    var genericParams = string.Join(", ", t.GetGenericArguments().Select(a => a.Name));
                    className += $"<{genericParams}>";
                }

                var fileContent = $$"""
namespace MoreHttpApi.Shared.Specs;

[GeneratedCode("{{nameof(MoreHttpApi)}}", "10.0.0")]
[ComponentSpec("{{t.FullName}}")]
public record {{className}}(
{{propList}}
) : ParsedComponentSpec{{(idProp is not null ? ", IComponentSpecWithId" : ";")}}
""";

                if (idProp is not null)
                {
                    if (idProp == "Id")
                    {
                        fileContent += ";";
                    }
                    else
                    {
                        fileContent += $$"""

{
    public string Id => {{idProp}};
}
""";
                    }
                }

                File.WriteAllText(filePath, fileContent);



                additionalTypes.Remove(t);
                types.Add(t);
            }
        }

        GenerateEnums(pendingEnums, outputFolder);
    }

    static void GenerateEnums(IEnumerable<Type> enums, string outputFolder)
    {
        foreach (var t in enums)
        {
            var name = GetParsedTypeName(t);
            var filePath = Path.Combine(outputFolder, name + ".cs");

            var values = string.Join(Environment.NewLine, Enum.GetValues(t).Cast<object>().Select(v => $"    {v} = {(int)v},"));
            File.WriteAllText(filePath, $$"""
namespace MoreHttpApi.Shared.Specs;

[GeneratedCode("{{nameof(MoreHttpApi)}}", "10.0.0")]
public enum {{name}}
{
{{values}}
}
""");
        }
    }

    static string? DeterminePropertyType(Type pType, HashSet<Type> otherTypes, HashSet<Type> pendingTypes, HashSet<Type> pendingEnums)
    {
        if (pType.IsGenericParameter)
        {
            return pType.Name;
        }

        if (pType.IsPrimitive)
        {
            return pType.Name;
        }

        if (pType.IsEnum)
        {
            pendingEnums.Add(pType);
            return GetParsedTypeName(pType);
        }

        if (otherTypes.Contains(pType) || pendingTypes.Contains(pType))
        {
            return GetParsedTypeName(pType);
        }

        if (SkippedTypes.Contains(pType))
        {
            return null;
        }

        if (StringTypes.Contains(pType))
        {
            return "string";
        }

        if (SerializableIntTypes.Contains(pType))
        {
            return "HttpSerializableInts";
        }

        if (SerializableFloatTypes.Contains(pType))
        {
            return "HttpSerializableFloats";
        }

        var isGeneric = pType.IsGenericType;
        var isRecordClass = IsRecordClass(pType);

        if (isGeneric)
        {
            var genericDef = pType.GetGenericTypeDefinition();

            if (genericDef == typeof(AssetRef<>))
            {
                return "string";
            }

            if (genericDef == typeof(ImmutableArray<>))
            {
                return DeterminePropertyType(pType.GetGenericArguments()[0], otherTypes, pendingTypes, pendingEnums) + "[]";
            }

            // A generic like MinMaxSpec<T>
            AddPendingType(genericDef);
            var genericArgs = string.Join(", ", pType.GetGenericArguments().Select(a => DeterminePropertyType(a, otherTypes, pendingTypes, pendingEnums)));
            return $"{GetParsedTypeName(genericDef)}<{genericArgs}>";
        }

        if (isRecordClass)
        {
            AddPendingType(pType);
            return GetParsedTypeName(pType);
        }

        throw new NotSupportedException();

        void AddPendingType(Type type)
        {
            if (!otherTypes.Contains(type))
            {
                pendingTypes.Add(type);
            }
        }
    }

    static bool IsRecordClass(Type t) => t.IsClass && t.GetMethod("<Clone>$") is not null;

    static string GetParsedTypeName(Type type) => "Parsed" + GetCleanGenericName(type.Name);

    static string GetCleanGenericName(string name)
    {
        var backtickIndex = name.IndexOf('`');
        return backtickIndex >= 0 ? name[..backtickIndex] : name;
    }

}
